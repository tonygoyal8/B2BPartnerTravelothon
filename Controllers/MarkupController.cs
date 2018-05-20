using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Markup;
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
    public class MarkupController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [ResponseType(typeof(ObjectDto<List<MarkupDto>>))]
        public async Task<IHttpActionResult> GetMarkups()
        {
            var messages = new Messages();
            ObjectDto<List<MarkupDto>> result = new ObjectDto<List<MarkupDto>>();
            var userId = User.Identity.GetUserId();
            var markups = await db.Markups.Where(x => x.UserId == userId).ToListAsync();
            if (markups == null || markups.Count() == 0)
            {
                messages.Message = "No record found";
                result.messages.Add(messages);
            }
            else
            {
                result.valid = true;
                result.Object = markups;
            }

            return Ok(result);
        }

        // PUT: api/Bank/5
        [ResponseType(typeof(ObjectDto<MarkupDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateMarkup(MarkupDto markup)
        {
            var messages = new Messages();
            ObjectDto<MarkupDto> result = new ObjectDto<MarkupDto>();
            markup.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            db.Entry(markup).State = EntityState.Modified;

            try
            {
                if ((await db.SaveChangesAsync()) > 0)
                {
                    result.Object = markup;
                    result.valid = true;
                    messages.Message = "Markup updated successfully";
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
        [ResponseType(typeof(ObjectDto<MarkupDto>))]
        [HttpPost]
        public async Task<IHttpActionResult> AddMarkup(MarkupDto model)
        {
            var messages = new Messages();
            ObjectDto<MarkupDto> result = new ObjectDto<MarkupDto>();
            try
            {
                var userId = User.Identity.GetUserId();
                model.UserId = userId;
                model.CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                model.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                db.Markups.Add(model);
                if ((await db.SaveChangesAsync()) > 0)
                {
                    result.Object = model;
                    result.valid = true;
                    messages.Message = "Markup added successfully";
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
        public async Task<IHttpActionResult> DeleteMarkup(int id)
        {
            MarkupDto markup = await db.Markups.FindAsync(id);
            var messages = new Messages();
            ObjectDto<string> result = new ObjectDto<string>();
            if (markup == null)
            {
                messages.Message = "No record found";
                result.messages.Add(messages);
            }
            else
            {
                try
                {
                    db.Markups.Remove(markup);
                    if ((await db.SaveChangesAsync()) > 0)
                    {
                        result.valid = true;
                        messages.Message = "Markup deleted successfully";
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

        private bool MarkupExists(int id)
        {
            return db.Markups.Count(e => e.Id == id) > 0;
        }
    }
}
