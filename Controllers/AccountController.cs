using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Providers;
using B2BPartnerTravelothon.Results;
using B2BPartnerTravelothon.ViewModel.Shared;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Linq;
using B2BPartnerTravelothon.Models.User;
using System.Data.Entity;
using B2BPartnerTravelothon.Repository.Email;
using B2BPartnerTravelothon.Constants;
using System.Web.Http.Description;
using B2BPartnerTravelothon.ViewModel.Users;

namespace B2BPartnerTravelothon.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            var messages = new Messages();
            ObjectDto<object> data = new ObjectDto<object>();
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        foreach (var errors in modelError.Value.Errors)
                        {
                            messages.Message = errors.ErrorMessage;
                            data.messages.Add(messages);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                        model.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {

                            messages.Message = "*" + error;
                        }
                        data.messages.Add(messages);
                    }
                    else
                    {
                        data.valid = true;
                        messages.Type = "SUCCESS";
                        messages.Message = "Your password has been reset";
                        data.messages.Add(messages);
                    }
                }
                catch (Exception e)
                {
                    messages.Message = e.Message;
                    data.messages.Add(messages);
                }
            }
            return Ok(data);
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            var messages = new Messages();
            ObjectDto<object> data = new ObjectDto<object>();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);


            if (!result.Succeeded)
            {
                foreach (string error in result.Errors)
                {
                    messages.Message = "*" + error;
                }
                data.messages.Add(messages);
            }
            else
            {
                data.valid = true;
            }
            return Ok(data);
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserProfile model)
        {
            var messages = new Messages();
            ObjectDto<object> data = new ObjectDto<object>();
            //var isValidEmail = IsValidEmail(model.Email);
            //if (!isValidEmail)
            //{
            //    var message = new Messages();
            //    message.Message = "*Invalid Email Id";
            //    data.messages.Add(message);
            //}
            //var isValidMobileNumber = IsValidMobileNumber(model.PhoneNumber);
            //if (!isValidMobileNumber)
            //{
            //    var message = new Messages();
            //    message.Message = "*Invalid Mobile Number";
            //    data.messages.Add(message);
            //}
            //var isValidPassword = IsValidPassword(model.Password);
            //if (!isValidPassword)
            //{
            //    var message = new Messages();
            //    message.Message = "*Invalid Password";
            //    data.messages.Add(message);
            //}

            //if (data.messages.Count > 0)
            //{
            //    return Ok(data);
            //}
            try
            {
                using (var con = new ApplicationDbContext())
                {
                    var usedPhoneNumber = con.Users.FirstOrDefault(x => x.PhoneNumber == model.PhoneNumber);
                    if (usedPhoneNumber != null)
                    {
                        var message = new Messages();
                        message.Message = "*Phone Number '" + model.PhoneNumber + "' is already taken.";
                        data.messages.Add(message);
                        return Ok(data);
                    }
                }

                var user = new ApplicationUser() { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    //return GetErrorResult(result);
                    foreach (string error in result.Errors)
                    {
                        messages.Message = "*" + error;
                    }
                    data.messages.Add(messages);
                }
                else
                {
                    await UserManager.AddToRoleAsync(user.Id, model.Role);
                    data.valid = true;
                    var Body = "<div style='font-family: Verdana;font-size:12px'>Dear " + model.FirstName + " " + model.LastName + ",";
                    Body += "<br/><br/>Welcome to the Travelothon family. Now access best in class online services.";
                    Body += "<br/><br/>Below are your details:<br/><br/>username: " + model.Email + "<br/>password: " + model.Password;
                    Body += "<br/><br/><a href='partner.travelothon.in'>Visit Partner Portal</a>";

                    var Subject = "Welcome to Travelothon";

                    var emailService = new EMail();
                    await emailService.SendAsync(new IdentityMessage()
                    {
                        Body = Body,
                        Subject = Subject,
                        Destination = model.Email
                    });
                    using (var con = new ApplicationDbContext())
                    {
                        try
                        {
                            var tUserProfile = new PUserProfileDto
                            {
                                Aadhar = model.Aadhar,
                                Agency = model.Agency,
                                Balance = 0,
                                BirthDate = model.BirthDate,
                                City = model.City,
                                Country = model.Country,
                                CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                LastName = model.LastName,
                                LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                State = model.State,
                                GST = model.GST,
                                FirstName = model.FirstName,
                                Gender = model.Gender,
                                PanCard = model.PanCard,
                                ParentId = model.ParentId,
                                PaymentId = model.PaymentId,
                                PlanId = model.PlanId,
                                UserId = user.Id,
                                Email = model.Email,
                                PhoneNumber = model.PhoneNumber
                            };
                            con.PUserProfile.Add(tUserProfile);
                            con.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            var error = e.Message + "\n" + e.GetBaseException() + "\n" + e.InnerException + "\n" + e.Data;

                            var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                            emailService.SendAsync(new IdentityMessage()
                            {
                                Body = error,
                                Subject = "API:B2B UserRegistration Error.Environment "+ Request.RequestUri.Host.ToString(),
                                Destination = Destination
                            });
                            messages.Message = e.Message;
                            data.messages.Add(messages);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException() + e.InnerException + "\n" + e.Data,
                Subject = "B2B User Registration Error. Environment: " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                data.messages.Add(new Messages
                {
                    Message = e.Message
                });
            }
            return Ok(data);

        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IHttpActionResult> UpdateProfile(PUserProfileDto model)
        {
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            if (String.IsNullOrEmpty(model.PanCard) || String.IsNullOrEmpty(model.LastName) || String.IsNullOrEmpty(model.FirstName) ||
                String.IsNullOrEmpty(model.Country) || String.IsNullOrEmpty(model.State) || String.IsNullOrEmpty(model.City))
            {
                messages.Message = "*All fields are required";
                result.messages.Add(messages);
            }
            else
            {
                try
                {
                    model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    using (var context = new ApplicationDbContext())
                    {

                        var user = await context.PUserProfile.FindAsync(model.UserId);

                        user.BirthDate = model.BirthDate;
                        user.City = model.City;
                        user.Country = model.Country;
                        user.FirstName = model.FirstName;
                        user.Gender = model.Gender;
                        user.LastName = model.LastName;
                        user.PanCard = model.PanCard;
                        user.State = model.State;
                        user.GST = model.GST;

                        context.Entry(user).State = EntityState.Modified;
                        int i = await context.SaveChangesAsync();
                        if (i == 0)
                        {
                            messages.Message = "Internal Server Error";
                            result.messages.Add(messages);
                        }
                        else
                        {
                            messages.Message = "Your profile has updated successfully";
                            messages.Type = Toaster.SUCCESS.ToString();
                            result.messages.Add(messages);
                            result.valid = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    messages.Message = e.Message + " " + e.GetBaseException();
                    result.messages.Add(messages);
                }
            }
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> ForgotPassword(string email, string url)
        {
            ObjectDto<string> result = new ObjectDto<string>();
            var messages = new Messages();
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                messages.Message = "There is no user registered with that email address";
                result.messages.Add(messages);
            }
            else
            {
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = url + "/#/reset-password?id=" + user.Id + "&token=" + code;
                var resetLink = "<div style='font-family: Verdana;font-size:12px'>Dear " + email.Split('@')[0].ToUpper() + ",";
                resetLink += "<br/><br/>This mail is in response to a request made on Travelothon to reset your password.";
                resetLink += "<br/><br/>You can reset your password by clicking on below link";
                resetLink += "<br/><br/><a href='" + callbackUrl + "'>Reset Now</a></div>";

                var Subject = "Password Reset";
                var Body = resetLink;

                var emailService = new EMail();
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = Body,
                    Subject = Subject,
                    Destination = email
                });

                result.valid = true;
                messages.Message = "Check your e-mail for the confirmation link";
                messages.Type = Toaster.SUCCESS.ToString();
                result.messages.Add(messages);
            }
            return Ok(result);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordModel model)
        {
            ObjectDto<string> result = new ObjectDto<string>();
            var messages = new Messages();
            if (ModelState.IsValid)
            {
                var resetResponse = await UserManager.ResetPasswordAsync(model.userId, model.ReturnToken, model.Password);
                if (resetResponse.Succeeded)
                {

                    messages.Message = "Your password has changed successfully";
                    messages.Type = Toaster.SUCCESS.ToString();
                    result.messages.Add(messages);
                    result.valid = true;
                }
                else
                {
                    messages.Message = "Something went horribly wrong!";
                    result.messages.Add(messages);
                }
            }
            else
            {
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        foreach (var errors in modelError.Value.Errors)
                        {
                            messages.Message = errors.ErrorMessage;
                            result.messages.Add(messages);
                        }
                    }
                }
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("RegisterUser")]
        [ResponseType(typeof(ObjectDto<decimal>))]
        public async Task<IHttpActionResult> RegisterUser(UserProfile model)
        {
            var messages = new Messages();
            ObjectDto<decimal> data = new ObjectDto<decimal>();
            try
            {
                decimal balance = 0;
                var UId = User.Identity.GetUserId();
                using (var con = new ApplicationDbContext())
                {
                    var usedPhoneNumber = con.Users.FirstOrDefault(x => x.PhoneNumber == model.PhoneNumber);
                    if (usedPhoneNumber != null)
                    {
                        var message = new Messages();
                        message.Message = "*Phone Number '" + model.PhoneNumber + "' is already taken.";
                        data.messages.Add(message);
                        return Ok(data);
                    }
                    var plan = await con.PUserRegistrationPlans.FindAsync(model.PlanId);
                    var User = await con.PUserProfile.FindAsync(UId);
                    var AId = System.Configuration.ConfigurationManager.AppSettings.Get("PartnerLoginId");
                    var admin = await con.PUserProfile.FirstOrDefaultAsync(x=>x.Email.ToUpper()==AId.ToUpper());
                    if (plan != null)
                    {
                        if (User.Balance < plan.Fee)
                        {
                            var message = new Messages();
                            message.Message = "*Your account balance is low";
                            data.messages.Add(message);
                            return Ok(data);
                        }
                        User.Balance -= Convert.ToDecimal(plan.Fee);
                        model.Balance = plan.BalanceAmount;
                        var adminBal = Convert.ToDecimal(plan.Fee) - plan.BalanceAmount;
                        User.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        con.Entry(User).State = EntityState.Modified;
                        admin.Balance += adminBal;
                        admin.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        con.Entry(admin).State = EntityState.Modified;
                        if ((await con.SaveChangesAsync()) <= 0)
                        {
                            var message = new Messages();
                            message.Message = "*Internal Server Error";
                            data.messages.Add(message);
                            return Ok(data);
                        }
                        balance = User.Balance;
                    }
                    model.ParentId = User.Id;
                    model.Password = "123456";
                }

                var user = new ApplicationUser() { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    //return GetErrorResult(result);
                    foreach (string error in result.Errors)
                    {
                        messages.Message = "*" + error;
                    }
                    data.messages.Add(messages);
                }
                else
                {
                    await UserManager.AddToRoleAsync(user.Id, model.Role);
                    data.valid = true;
                    data.Object = balance;
                    var Body = "<div style='font-family: Verdana;font-size:12px'>Dear " + model.FirstName + " " + model.LastName + ",";
                    Body += "<br/><br/>Welcome to the Travelothon family. Now access best in class online services.";
                    Body += "<br/><br/>Below are your details:<br/><br/>username: "+model.Email+"<br/>password: "+model.Password;
                    Body += "<br/><br/><a href='partner.travelothon.in'>Visit Partner Portal</a>";


                    var Subject = "Welcome to Travelothon";

                    var emailService = new EMail();
                    emailService.SendAsync(new IdentityMessage()
                    {
                        Body = Body,
                        Subject = Subject,
                        Destination = model.Email
                    });
                    using (var con = new ApplicationDbContext())
                    {
                        try
                        {
                            var tUserProfile = new PUserProfileDto
                            {
                                Aadhar = model.Aadhar,
                                Agency = model.Agency,
                                Balance = model.Balance,
                                BirthDate = model.BirthDate,
                                City = model.City,
                                Country = model.Country,
                                CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                LastName = model.LastName,
                                LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                State = model.State,
                                GST = model.GST,
                                FirstName = model.FirstName,
                                Gender = model.Gender,
                                PanCard = model.PanCard,
                                ParentId = model.ParentId,
                                PaymentId = model.PaymentId,
                                PlanId = model.PlanId,
                                UserId = user.Id,
                                Email = model.Email,
                                PhoneNumber = model.PhoneNumber
                            };
                            con.PUserProfile.Add(tUserProfile);
                            con.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            var dd = e.Message + e.GetBaseException();
                            var error = e.Message + "\n" + e.GetBaseException() + "\n" + e.InnerException + "\n" + e.Data;

                            var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                            await emailService.SendAsync(new IdentityMessage()
                            {
                                Body = error,
                                Subject = "API:B2B UserRegistration Error,Environment:" + Request.RequestUri.Host.ToString(),
                                Destination = Destination
                            });
                            messages.Message = e.Message;
                            data.messages.Add(messages);
                        }
                    }

                }
            }
            catch(Exception e)
            {
                var dd = e.Message + e.GetBaseException();
                var error = e.Message + "\n" + e.GetBaseException() + "\n" + e.InnerException + "\n" + e.Data;
                var emailService = new EMail();

                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = error,
                    Subject = "API:B2B UserRegistration Error,Environment:" + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                messages.Message = e.Message;
                data.messages.Add(messages);
            }
            return Ok(data);
        }

        [Route("GetUsers")]
        [ResponseType(typeof(ObjectDto<List<PUserProfileDto>>))]
        public async Task<IHttpActionResult> GetUsers()
        {
            var messages = new Messages();
            ObjectDto<List<PUserProfileDto>> data = new ObjectDto<List<PUserProfileDto>>();
            try
            {
                var UId = User.Identity.GetUserId();
                using (var con = new ApplicationDbContext())
                {
                    var userObj = await con.PUserProfile.FindAsync(UId);
                    var children = await con.PUserProfile.Where(x => x.ParentId == userObj.Id).ToListAsync();
                    if (children != null && children.Count() > 0)
                    {
                        data.Object = children;
                        data.valid = true;
                    }
                    else
                    {
                        messages.Message = "No record found";
                        data.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.InnerException;
                data.messages.Add(messages);
            }
            return Ok(data);
        }

        [Route("GetAllUsers/{role}")]
        [Authorize(Roles = Roles.AD)]
        [ResponseType(typeof(ObjectDto<List<UsersReport>>))]
        public async Task<IHttpActionResult> GetAllUsers(string role)
        {
            var messages = new Messages();
            ObjectDto<List<UsersReport>> data = new ObjectDto<List<UsersReport>>();
            try
            {
                using (var con = new ApplicationDbContext())
                {
                    var roles = con.Roles.FirstOrDefault(x => x.Name.ToUpper() == role.ToUpper());
                    var users =roles.Users.Select(x=>x.UserId);
                    var userDto= await con.PUserProfile.Where(p => users.Any(x=>x==p.UserId))
                                    .ToListAsync();

                    if (userDto != null && userDto.Count() > 0)
                    {
                        var userList = userDto.Select(k => new UsersReport(k, con.PUserProfile.FirstOrDefault(x => x.Id == k.ParentId).Agency)).OrderBy(f=>f.Name).ToList();
                        data.Object = userList;
                        data.valid = true;
                    }
                    else
                    {
                        messages.Message = "No record found";
                        data.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.InnerException;
                data.messages.Add(messages);
            }
            return Ok(data);
        }




        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }


        private bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private bool IsValidMobileNumber(string mobileNumber)
        {
            try
            {
                return mobileNumber.Length == 10;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private bool IsValidPassword(string password)
        {
            try
            {
                Regex r = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$");

                return password.Length > 5;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRegistrationPlans")]
        [ResponseType(typeof(ObjectDto<List<PUserRegistrationPlansDto>>))]
        public async Task<IHttpActionResult> GetRegistrationPlans()
        {
            ObjectDto<List<PUserRegistrationPlansDto>> result = new ObjectDto<List<PUserRegistrationPlansDto>>();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    result.Object = await context.PUserRegistrationPlans.ToListAsync();
                    if (result.Object == null || result.Object.Count == 0)
                    {
                        var messages = new Messages();
                        messages.Message = "No Plan Found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.valid = true;
                    }
                }
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetBalance")]
        [ResponseType(typeof(ObjectDto<decimal>))]
        public async Task<IHttpActionResult> GetBalance()
        {
            ObjectDto<decimal> result = new ObjectDto<decimal>();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var UId = User.Identity.GetUserId();
                    var userObj = await context.PUserProfile.FindAsync(UId);
                    result.Object = userObj.Balance;
                    result.valid = true;
                }
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return Ok(result);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}
