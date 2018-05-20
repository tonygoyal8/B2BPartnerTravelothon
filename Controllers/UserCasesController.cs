using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Cases;
using B2BPartnerTravelothon.Models.Passbook;
using B2BPartnerTravelothon.ViewModel.Cases;
using B2BPartnerTravelothon.ViewModel.Shared;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace B2BPartnerTravelothon.Controllers
{
    [Authorize]
    public class UserCasesController : ApiController
    {
        [ResponseType(typeof(ObjectDto<List<UserCasesViewModel>>))]
        public async Task<IHttpActionResult> GetMyCases()
        {
            var messages = new Messages();
            ObjectDto<List<UserCasesViewModel>> result = new ObjectDto<List<UserCasesViewModel>>();
            var UId = User.Identity.GetUserId();

            using (var con = new ApplicationDbContext())
            {
                var casesDto = await con.UserCases.Where(x => x.UserId == UId).ToListAsync();
                if (casesDto == null || casesDto.Count() == 0)
                {
                    messages.Message = "No record found";
                    result.messages.Add(messages);
                }
                else
                {
                    var passbook = casesDto.Select(p => new UserCasesViewModel(p)).OrderByDescending(x => x.Date).ToList();
                    result.Object = passbook;
                    result.valid = true;
                }

            }

            return Ok(result);
        }

        [Authorize(Roles =Roles.AD)]
        [ResponseType(typeof(ObjectDto<List<UserCasesViewModel>>))]
        public async Task<IHttpActionResult> GetUsersCase()
        {
            var messages = new Messages();
            ObjectDto<List<UserCasesViewModel>> result = new ObjectDto<List<UserCasesViewModel>>();
           
            var startdate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).AddDays(-7);
            var enddate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            using (var con = new ApplicationDbContext())
            {
                var casesDto = await (from uc in con.UserCases
                                  join pu in con.PUserProfile on uc.UserId equals pu.UserId
                                  where uc.CreatedDate >= startdate && uc.CreatedDate <= enddate
                                  select new { pu, uc }).ToListAsync();

                var cases = casesDto.Select(p => new UserCasesViewModel(p.uc,p.pu)).OrderByDescending(x => x.Date).ToList();

                if (cases == null || cases.Count() == 0)
                {
                    messages.Message = "No record found";
                    result.messages.Add(messages);
                }
                else
                {
                    result.Object = cases;
                    result.valid = true;
                }

            }
            return Ok(result);
        }

        [Authorize(Roles = Roles.TA)]
        [ResponseType(typeof(ObjectDto<string>))]
        public async Task<IHttpActionResult> SubmitCase(UserCasesDto model)
        {
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            model.UserId = User.Identity.GetUserId();
            model.Status =(int)UserCasesStatus.Inprogress;

            model.CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //var UEmail = User.Identity.GetUserName();
            model.Remarks = "Partner: " + model.Remarks;
            using (var con = new ApplicationDbContext())
            {
                var isValidCase =await con.PFlights.FindAsync(model.TransactionId);
                if (isValidCase != null && isValidCase.UserId==model.UserId)
                {
                    con.UserCases.Add(model);
                    if ((await con.SaveChangesAsync()) > 0)
                    {
                        messages.Message = "Thank you for contacting us.Our support team will contact you shortly.";
                        messages.Type = Toaster.SUCCESS.ToString();
                        result.messages.Add(messages);
                        result.valid = true;
                    }
                    else
                    {
                        messages.Message = "Unable to submit your case.Kindly try again after sometime.";
                        result.messages.Add(messages);
                        result.valid = false;
                    }
                }
                else
                {
                    messages.Message="Unable to create a case of invalid trasaction.";
                    result.messages.Add(messages);
                    result.valid = false;
                }
            }
            return Ok(result);
        }

        [Authorize(Roles = Roles.AD)]
        [ResponseType(typeof(ObjectDto<string>))]
        public async Task<IHttpActionResult> UpdateCase(UserCasesViewModel model)
        {
            if (model.Refund != 0)
            {
                return Ok(await Refund(model));
            }
            else
            {
                return Ok(await Charges(model));
            }
        }

        public async Task<ObjectDto<string>> Charges(UserCasesViewModel model)
        {
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            using (var db = new ApplicationDbContext())
            {
                var userCaseDto = await db.UserCases.FindAsync(model.Id);

                var userId = User.Identity.GetUserId();
                var userObj = await db.PUserProfile.FindAsync(userId);
                var childObj = await db.PUserProfile.FindAsync(userCaseDto.UserId);
                if (childObj.Balance >= model.Charged)
                {
                    userObj.Balance += model.Charged;
                    childObj.Balance -= model.Charged;
                    userCaseDto.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    childObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                    userCaseDto.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    userCaseDto.Status = (int)UserCasesStatus.Closed;
                    userCaseDto.Remarks += "<br/>Admin:" + model.Remarks;
                    userCaseDto.Charged = model.Charged;

                    db.Entry(userCaseDto).State = EntityState.Modified;
                    db.Entry(userObj).State = EntityState.Modified;
                    db.Entry(childObj).State = EntityState.Modified;

                    var childPassBook = new PassBookDto
                    {
                        Amount = model.Charged,
                        Balance = childObj.Balance,
                        Credit = 0,
                        Debit = model.Charged,
                        ServiceId = userCaseDto.ServiceTypeId,
                        Status = (int)StatusFlag.Success,
                        TransactionId = model.TransactionId,
                        UserId = childObj.UserId,
                        Remarks = ((UserCaseReason)userCaseDto.Issue).ToString().Replace("_"," ") +" Charges",
                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    };
                    var userPassBook = new PassBookDto
                    {
                        Amount = model.Charged,
                        Balance = userObj.Balance,
                        Credit = model.Charged,
                        Debit = 0,
                        Status = (int)StatusFlag.Success,
                        TransactionId = model.TransactionId,
                        UserId = userObj.UserId,
                        Remarks = ((UserCaseReason)userCaseDto.Issue).ToString().Replace("_"," ") +" Charges",
                        ServiceId = userCaseDto.ServiceTypeId,
                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    };

                    db.PassBook.Add(userPassBook);
                    db.PassBook.Add(childPassBook);
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.valid = true;
                        messages.Message = "Case closed & amount has been charged to " + model.Agency;
                        messages.Type = Toaster.SUCCESS.ToString();
                        result.messages.Add(messages);
                    }
                    else
                    {
                        messages.Message = "Internal Server Error.";
                        result.messages.Add(messages);
                    }
                }
                else
                {
                    messages.Message = "Not enough balance is available in agent's account";
                    result.messages.Add(messages);
                }
            }
            return result;
        }
        public async Task<ObjectDto<string>> Refund(UserCasesViewModel model)
        {
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            using (var db = new ApplicationDbContext())
            {
                var userCaseDto = await db.UserCases.FindAsync(model.Id);

                var userId = User.Identity.GetUserId();
                var userObj = await db.PUserProfile.FindAsync(userId);
                var childObj = await db.PUserProfile.FindAsync(userCaseDto.UserId);
                if (userObj.Balance >= model.Refund)
                {
                    userObj.Balance -= model.Refund;
                    childObj.Balance += model.Refund;

                    userCaseDto.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    childObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                    userCaseDto.Status = (int)UserCasesStatus.Closed;
                    userCaseDto.Remarks += "<br/>Admin:" + model.Remarks;
                    userCaseDto.Refund = model.Refund;

                    db.Entry(userCaseDto).State = EntityState.Modified;
                    db.Entry(userObj).State = EntityState.Modified;
                    db.Entry(childObj).State = EntityState.Modified;

                    var childPassBook = new PassBookDto
                    {
                        Amount = model.Refund,
                        Balance = childObj.Balance,
                        Credit = model.Refund,
                        Debit = 0,
                        ServiceId = userCaseDto.ServiceTypeId,
                        Status = (int)StatusFlag.Success,
                        TransactionId = model.TransactionId,
                        UserId = childObj.UserId,
                        Remarks = ((UserCaseReason)userCaseDto.Issue).ToString().Replace("_", " ") + " Refund",
                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    };
                    var userPassBook = new PassBookDto
                    {
                        Amount = model.Refund,
                        Balance = userObj.Balance,
                        Credit = 0,
                        Debit = model.Refund,
                        Status = (int)StatusFlag.Success,
                        TransactionId = model.TransactionId,
                        UserId = userObj.UserId,
                        Remarks = ((UserCaseReason)userCaseDto.Issue).ToString().Replace("_", " ") + " Refund",
                        ServiceId = userCaseDto.ServiceTypeId,
                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    };

                    db.PassBook.Add(userPassBook);
                    db.PassBook.Add(childPassBook);
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.valid = true;
                        messages.Message = "Case closed & amount has been refunded to "+model.Agency+"'s account";
                        messages.Type = Toaster.SUCCESS.ToString();
                        result.messages.Add(messages);
                    }
                    else
                    {
                        messages.Message = "Internal Server Error.";
                        result.messages.Add(messages);
                    }
                }
                else
                {
                    messages.Message = "Not enough balance is available in your account";
                    result.messages.Add(messages);
                }
            }
            return result;
        }
    }
}
