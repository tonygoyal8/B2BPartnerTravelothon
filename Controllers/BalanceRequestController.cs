using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Bank;
using B2BPartnerTravelothon.Models.Passbook;
using B2BPartnerTravelothon.ViewModel.BalanceRequest;
using B2BPartnerTravelothon.ViewModel.Shared;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace B2BPartnerTravelothon.Controllers
{
    [Authorize]
    public class BalanceRequestController : ApiController
    {
        [ResponseType(typeof(ObjectDto<List<PBanksDto>>))]
        public async Task<IHttpActionResult> GetParentBanks()
        {
            var messages = new Messages();
            ObjectDto<List<PBanksDto>> result = new ObjectDto<List<PBanksDto>>();
            var userId = User.Identity.GetUserId();
            using (var con = new ApplicationDbContext())
            {
                var userObj = await con.PUserProfile.FindAsync(userId);
                var parentUserId = con.PUserProfile.FirstOrDefault(x => x.Id == userObj.ParentId).UserId;
                var banks = con.Banks.Where(x => x.UserId == parentUserId).ToList();
                if (banks == null || banks.Count() == 0)
                {
                    messages.Message = "No record found";
                    result.messages.Add(messages);
                }
                else
                {
                    result.valid = true;
                    result.Object = banks;
                }
                return Ok(result);
            }
        }

        [ResponseType(typeof(ObjectDto<PBalanceRequestDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> CreateRequest(PBalanceRequestDto model)
        {
            var messages = new Messages();
            ObjectDto<PBalanceRequestDto> result = new ObjectDto<PBalanceRequestDto>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    model.UserId = userId;
                    model.CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    model.Status = (int)BalanceRequestFlag.Inprogress;

                    db.BalanceRequests.Add(model);
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.Object = model;
                        result.valid = true;
                        messages.Message = "Your request has been submitted Successfully";
                        messages.Type = Toaster.SUCCESS.ToString();
                        result.messages.Add(messages);
                    }
                    else
                    {
                        messages.Message = "Internal Server Error";
                        result.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<string>))]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteBalanceRequest(int id)
        {
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            using (var db = new ApplicationDbContext())
            {
                var balReq = await db.BalanceRequests.FindAsync(id);

                if (balReq == null)
                {
                    messages.Message = "No record found";
                    result.messages.Add(messages);
                }
                else
                {
                    if (balReq.Status == (int)BalanceRequestFlag.Inprogress)
                    {
                        try
                        {
                            db.BalanceRequests.Remove(balReq);
                            if ((await db.SaveChangesAsync()) > 0)
                            {
                                result.valid = true;
                                messages.Message = "Your request for balance has been deleted successfully";
                                messages.Type = Toaster.SUCCESS.ToString();
                                result.messages.Add(messages);
                            }
                            else
                            {
                                messages.Message = "Internal Server Error";
                                result.messages.Add(messages);
                            }
                        }
                        catch (Exception e)
                        {
                            messages.Message = e.Message;
                            result.messages.Add(messages);
                        }
                    }
                    else
                    {
                        messages.Message = "Unable to delete the processed balance request";
                        result.messages.Add(messages);
                    }
                }
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<PBalanceRequestDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateRequestStatus(BalanceRequestViewModel model)
        {
            if (model.Status == (int)BalanceRequestFlag.Approved)
            {
                return Ok(await ApproveBalance(model));
            }
            else
            {
                return Ok(await DenyBalance(model));
            }
        }

        #region Private Approve/Deny Balance Request Methods
        private async Task<ObjectDto<BalanceRequestViewModel>> ApproveBalance(BalanceRequestViewModel model)
        {
            if (model.Purpose == (int)BalanceRequestPurpose.For_All_Services)
            {
                return (await ApproveBalanceforAllServices(model));
            }
            else if (model.Purpose == (int)BalanceRequestPurpose.From_Rail_To_Agent_Account)
            {
                return (await ApproveBalanceFromRailToAccount(model));
            }
            else
            {
                return (await ApproveBalanceFromAccountToRail(model));
            }
        }
        #region Balance Request Approved
        private async Task<ObjectDto<BalanceRequestViewModel>> ApproveBalanceforAllServices(BalanceRequestViewModel model)
        {
            var messages = new Messages();
            ObjectDto<BalanceRequestViewModel> result = new ObjectDto<BalanceRequestViewModel>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var userObj = await db.PUserProfile.FindAsync(userId);
                    var AId = System.Configuration.ConfigurationManager.AppSettings.Get("PartnerLoginId").ToUpper();
                    var childObj = await db.PUserProfile.FindAsync(model.UserId);
                    var AEmail = childObj.Email.ToUpper();
                    if (AId == AEmail)
                    {
                        userObj.Balance += model.AmountApproved;
                        userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        db.Entry(new PBalanceRequestDto(model)).State = EntityState.Modified;
                        db.Entry(userObj).State = EntityState.Modified;
                        if ((await db.SaveChangesAsync()) > 0)
                        {
                            result.Object = model;
                            result.valid = true;
                            messages.Message = "You have approved balance request for " + userObj.Agency;
                            messages.Type = Toaster.SUCCESS.ToString();
                            result.messages.Add(messages);
                        }
                        else
                        {
                            messages.Message = "Internal Server Error";
                            result.messages.Add(messages);
                        }
                    }
                    else if (userObj.Balance >= model.AmountApproved)
                    {
                        userObj.Balance -= model.AmountApproved;
                        childObj.Balance += model.AmountApproved;
                        model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        childObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        db.Entry(new PBalanceRequestDto(model)).State = EntityState.Modified;
                        db.Entry(userObj).State = EntityState.Modified;
                        db.Entry(childObj).State = EntityState.Modified;

                        var childPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = childObj.Balance,
                            Credit = model.AmountApproved,
                            Debit = 0,
                            ServiceId = 0,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = childObj.UserId,
                            Remarks = "Balance Approved",
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };
                        var userPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = userObj.Balance,
                            Credit = 0,
                            Debit = model.AmountApproved,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = userObj.UserId,
                            Remarks = "Balance Approved",
                            ServiceId = 0,
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };

                        db.PassBook.Add(userPassBook);
                        db.PassBook.Add(childPassBook);
                        if ((await db.SaveChangesAsync()) > 0)
                        {
                            result.Object = model;
                            result.valid = true;
                            messages.Message = "You have approved balance request for " + childObj.Agency;
                            messages.Type = Toaster.SUCCESS.ToString();
                            result.messages.Add(messages);
                        }
                        else
                        {
                            messages.Message = "Internal Server Error";
                            result.messages.Add(messages);
                        }
                    }
                    else
                    {
                        messages.Message = "Not Enough Balance is Available in your Account";
                        result.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return result;
        }

        [Authorize(Roles = Roles.AD)]
        private async Task<ObjectDto<BalanceRequestViewModel>> ApproveBalanceFromRailToAccount(BalanceRequestViewModel model)
        {
            var messages = new Messages();
            ObjectDto<BalanceRequestViewModel> result = new ObjectDto<BalanceRequestViewModel>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var userObj = await db.PUserProfile.FindAsync(userId);
                    var childObj = await db.PUserProfile.FindAsync(model.UserId);
                    if (userObj.Balance >= model.AmountApproved)
                    {
                        userObj.Balance -= model.AmountApproved;
                        childObj.Balance += model.AmountApproved;
                        model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        childObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        db.Entry(new PBalanceRequestDto(model)).State = EntityState.Modified;
                        db.Entry(userObj).State = EntityState.Modified;
                        db.Entry(childObj).State = EntityState.Modified;

                        var childPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = childObj.Balance,
                            Credit = model.AmountApproved,
                            Debit = 0,
                            ServiceId = 0,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = childObj.UserId,
                            Remarks = "Rail Balance Approved",
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };
                        var userPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = userObj.Balance,
                            Credit = 0,
                            Debit = model.AmountApproved,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = userObj.UserId,
                            Remarks = "Rail Balance Approved",
                            ServiceId = 0,
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };

                        db.PassBook.Add(userPassBook);
                        db.PassBook.Add(childPassBook);
                        if ((await db.SaveChangesAsync()) > 0)
                        {
                            result.Object = model;
                            result.valid = true;
                            messages.Message = "You have approved balance request for " + childObj.Agency;
                            messages.Type = Toaster.SUCCESS.ToString();
                            result.messages.Add(messages);
                        }
                        else
                        {
                            messages.Message = "Internal Server Error";
                            result.messages.Add(messages);
                        }
                    }
                    else
                    {
                        messages.Message = "Not Enough Balance is Available in your Account";
                        result.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return result;
        }

        [Authorize(Roles = Roles.AD)]
        private async Task<ObjectDto<BalanceRequestViewModel>> ApproveBalanceFromAccountToRail(BalanceRequestViewModel model)
        {
            var messages = new Messages();
            ObjectDto<BalanceRequestViewModel> result = new ObjectDto<BalanceRequestViewModel>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var userObj = await db.PUserProfile.FindAsync(userId);
                    var childObj = await db.PUserProfile.FindAsync(model.UserId);
                    if (childObj.Balance >= model.AmountApproved)
                    {
                        userObj.Balance += model.AmountApproved;
                        childObj.Balance -= model.AmountApproved;
                        model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        childObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        db.Entry(new PBalanceRequestDto(model)).State = EntityState.Modified;
                        db.Entry(userObj).State = EntityState.Modified;
                        db.Entry(childObj).State = EntityState.Modified;

                        var childPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = childObj.Balance,
                            Credit = 0,
                            Debit = model.AmountApproved,
                            ServiceId = 0,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = childObj.UserId,
                            Remarks = "Rail Balance Approved",
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };
                        var userPassBook = new PassBookDto
                        {
                            Amount = model.AmountApproved,
                            Balance = userObj.Balance,
                            Credit = model.AmountApproved,
                            Debit = 0,
                            Status = (int)StatusFlag.Success,
                            TransactionId = model.Id,
                            UserId = userObj.UserId,
                            Remarks = "Rail Balance Approved",
                            ServiceId = 0,
                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                        };

                        db.PassBook.Add(userPassBook);
                        db.PassBook.Add(childPassBook);
                        if ((await db.SaveChangesAsync()) > 0)
                        {
                            result.Object = model;
                            result.valid = true;
                            messages.Message = "You have approved rail balance request for " + childObj.Agency;
                            messages.Type = Toaster.SUCCESS.ToString();
                            result.messages.Add(messages);
                        }
                        else
                        {
                            messages.Message = "Internal Server Error";
                            result.messages.Add(messages);
                        }
                    }
                    else
                    {
                        messages.Message = "Not enough balance is available in agent's account";
                        result.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return result;
        }
        #endregion
        private async Task<ObjectDto<BalanceRequestViewModel>> DenyBalance(BalanceRequestViewModel model)
        {
            var messages = new Messages();
            ObjectDto<BalanceRequestViewModel> result = new ObjectDto<BalanceRequestViewModel>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var childObj = await db.PUserProfile.FindAsync(model.UserId);

                    model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    db.Entry(new PBalanceRequestDto(model)).State = EntityState.Modified;
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.Object = model;
                        result.valid = true;
                        messages.Message = "You have rejected balance request for " + childObj.Agency;
                        messages.Type = Toaster.SUCCESS.ToString();
                        result.messages.Add(messages);
                    }
                    else
                    {
                        messages.Message = "Internal Server Error";
                        result.messages.Add(messages);
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return result;
        }
        #endregion

        //Get all the balance requests, submitted by the logged in user
        [ResponseType(typeof(ObjectDto<List<PBalanceRequestDto>>))]
        public async Task<IHttpActionResult> GetBalanceRequests()
        {
            var messages = new Messages();
            ObjectDto<List<PBalanceRequestDto>> result = new ObjectDto<List<PBalanceRequestDto>>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var requests = await db.BalanceRequests.Where(x => x.UserId == userId).ToListAsync();
                    if (requests == null || requests.Count() == 0)
                    {
                        messages.Message = "No record found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.valid = true;
                        result.Object = requests.OrderByDescending(x => x.LastModifiedDate).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.InnerException;
                result.messages.Add(messages);
            }
            return Ok(result);
        }

        //Get ALL the balance requests, submitted by the child users
        [ResponseType(typeof(ObjectDto<List<BalanceRequestViewModel>>))]
        public async Task<IHttpActionResult> GetUsersBalanceRequests()
        {
            var AId = System.Configuration.ConfigurationManager.AppSettings.Get("PartnerLoginId").ToUpper();
            var UId = User.Identity.GetUserName().ToUpper();

            if (AId == UId)
            {
                return Ok(await GetBalanceRequestsForAdmin());
            }
            else
            {
                return Ok(await GetBalanceRequestsForAllUser());
            }
        }

        private async Task<ObjectDto<List<BalanceRequestViewModel>>> GetBalanceRequestsForAllUser()
        {
            var messages = new Messages();
            ObjectDto<List<BalanceRequestViewModel>> result = new ObjectDto<List<BalanceRequestViewModel>>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var pUserId = await db.PUserProfile.FindAsync(userId);
                    var childUsers = await db.PUserProfile.Where(x => x.ParentId == pUserId.Id).ToListAsync();
                    var purpose = (int)BalanceRequestPurpose.For_All_Services;

                    var ids = childUsers.Select(k => k.UserId).ToList();
                    var PbankList = await db.Banks.Where(x => x.UserId == userId).ToListAsync();

                    var childRequestsDto = await db.BalanceRequests.Where(x => ids.Contains(x.UserId) && x.Purpose == purpose).ToListAsync();

                    var childRequests = childRequestsDto.Select(x => new BalanceRequestViewModel(x,
                                                              childUsers.FirstOrDefault(s => s.UserId == x.UserId)?.Agency,
                                                              PbankList.FirstOrDefault(s => s.Id == x.PBankId)?.Name));

                    if (childRequests == null || childRequests.Count() == 0)
                    {
                        messages.Message = "No record found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.valid = true;
                        result.Object = childRequests.OrderByDescending(x => x.CreatedDate).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.GetBaseException();
                result.messages.Add(messages);
            }
            return result;
        }

        [Authorize(Roles =Roles.AD)]
        private async Task<ObjectDto<List<BalanceRequestViewModel>>> GetBalanceRequestsForAdmin()
        {
            var messages = new Messages();
            ObjectDto<List<BalanceRequestViewModel>> result = new ObjectDto<List<BalanceRequestViewModel>>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    var pUserId = await db.PUserProfile.FindAsync(userId);
                    var childUsers = await db.PUserProfile.ToListAsync();

                    var MDs = childUsers.Where(x => x.ParentId == pUserId.Id).ToList();

                    var ids = MDs.Select(k => k.UserId).ToList();

                    var PbankList = await db.Banks.Where(x => x.UserId == userId).ToListAsync();

                    var childRequestsDto = await db.BalanceRequests.Where(x => ids.Contains(x.UserId)
                                                                               || x.Purpose == (int)BalanceRequestPurpose.From_Rail_To_Agent_Account
                                                                               || x.Purpose == (int)BalanceRequestPurpose.From_Agent_Account_To_Rail).ToListAsync();

                    var childRequests = childRequestsDto.Select(x => new BalanceRequestViewModel(x,
                                                              childUsers.FirstOrDefault(s => s.UserId == x.UserId)?.Agency,
                                                              PbankList.FirstOrDefault(s => s.Id == x.PBankId)?.Name));

                    if (childRequests == null || childRequests.Count() == 0)
                    {
                        messages.Message = "No record found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.valid = true;
                        result.Object = childRequests.OrderByDescending(x => x.CreatedDate).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.GetBaseException();
                result.messages.Add(messages);
            }
            return result;
        }
    }
}

