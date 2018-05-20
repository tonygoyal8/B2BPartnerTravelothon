using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Bank;
using Microsoft.AspNet.Identity;
using B2BPartnerTravelothon.ViewModel.Shared;
using B2BPartnerTravelothon.Constants;

namespace B2BPartnerTravelothon.Controllers
{
    [Authorize]
    public class BankController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
      
        [ResponseType(typeof(ObjectDto<List<PBanksDto>>))]
        public async Task<IHttpActionResult> GetBanks()
        {
            var messages = new Messages();
            ObjectDto<List<PBanksDto>> result = new ObjectDto<List<PBanksDto>>();
            var userId = User.Identity.GetUserId();
            var pBanksDto = await db.Banks.Where(x=>x.UserId==userId).ToListAsync();
            if (pBanksDto == null || pBanksDto.Count()==0)
            {
                messages.Message = "No record found";
                result.messages.Add(messages);
            }
            else
            {
                result.valid = true;
                result.Object = pBanksDto;
            }

            return Ok(result);
        }

        // PUT: api/Bank/5
        [ResponseType(typeof(ObjectDto<PBanksDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateBank(PBanksDto pBanksDto)
        {
            var messages = new Messages();
            ObjectDto<PBanksDto> result = new ObjectDto<PBanksDto>();
            pBanksDto.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            db.Entry(pBanksDto).State = EntityState.Modified;

            try
            {
                if ((await db.SaveChangesAsync()) > 0)
                {
                    result.Object = pBanksDto;
                    result.valid = true;
                    messages.Message = "Bank's detail updated successfully";
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
            return Ok(result);
        }

        // POST: api/Bank
        [ResponseType(typeof(ObjectDto<PBanksDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> AddBank(PBanksDto model)
        {
            var messages = new Messages();
            ObjectDto<PBanksDto> result = new ObjectDto<PBanksDto>();
            try
            {
                var userId = User.Identity.GetUserId();
                model.UserId = userId;
                model.CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                db.Banks.Add(model);
                if ((await db.SaveChangesAsync()) > 0)
                {
                    result.Object = model;
                    result.valid = true;
                    messages.Message = "Bank's detail added successfully";
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
            return Ok(result);
        }

        // DELETE: api/Bank/5
        [ResponseType(typeof(ObjectDto<string>))]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteBank(int id)
        {
            PBanksDto pBanksDto = await db.Banks.FindAsync(id);
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            if (pBanksDto == null)
            {
                messages.Message = "No record found";
                result.messages.Add(messages);
            }
            else
            {
                try
                {
                    db.Banks.Remove(pBanksDto);
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.valid = true;
                        messages.Message = "Bank's detail deleted successfully";
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
            return Ok(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PBanksDtoExists(int id)
        {
            return db.Banks.Count(e => e.Id == id) > 0;
        }
    }
}