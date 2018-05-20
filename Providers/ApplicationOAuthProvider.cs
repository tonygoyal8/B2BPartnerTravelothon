using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.User;
using Newtonsoft.Json;
using System.Net.Mail;

namespace B2BPartnerTravelothon.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }
        public bool IsValidEmail(string emailaddress)
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
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            var isEmail = IsValidEmail(context.UserName);

            var userName = context.UserName;
            if (!isEmail)
            {
                using (var con = new ApplicationDbContext())
                {
                    userName = con.Users.FirstOrDefault(x => x.PhoneNumber == userName)?.UserName;
                }
            }

            if (userName == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ApplicationUser user = await userManager.FindAsync(userName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = CreateProperties(user);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(ApplicationUser userApp)
        {
            var user = new PUserProfileDto();
            var userName = userApp.UserName;
            string userId = userApp.Id;
            var roleId = userApp.Roles.FirstOrDefault(x => x.UserId == userId).RoleId;
            var role = "";
            IList<string> services = new List<string>();
            if (!String.IsNullOrEmpty(userId))
            {
                using (var con = new ApplicationDbContext())
                {
                    user = con.PUserProfile.Find(userId);
                    role = con.Roles.FirstOrDefault(x => x.Id == roleId).Name;
                    var plan = con.PUserRegistrationPlans.Find(user.PlanId);
                    if (plan != null)
                    {
                        if (plan.Rail)
                            services.Add("Rail");
                        if (plan.Recharge)
                            services.Add("Recharge");
                        if (plan.Flight)
                            services.Add("Flight");
                        if (plan.DMR)
                            services.Add("DMR");
                    }
                }
            }
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName },
                 { "user",JsonConvert.SerializeObject(user)},
                { "role",role},
                { "services",JsonConvert.SerializeObject(services)}

            };
            return new AuthenticationProperties(data);
        }
    }
}