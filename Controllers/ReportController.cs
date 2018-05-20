using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Repository.Email;
using B2BPartnerTravelothon.SqlView;
using B2BPartnerTravelothon.ViewModel.Reports;
using B2BPartnerTravelothon.ViewModel.Shared;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace B2BPartnerTravelothon.Controllers
{
    public class ReportController : ApiController
    {
        [ResponseType(typeof(ObjectDto<List<PassBookReport>>))]
        [Authorize]
        public async Task<IHttpActionResult> getPassbook()
        {
            var messages = new Messages();
            ObjectDto<List<PassBookReport>> result = new ObjectDto<List<PassBookReport>>();
            var UId = User.Identity.GetUserId();
            using (var con=new ApplicationDbContext())
            {
                var passbookDto = await con.PassBook.Where(x => x.UserId == UId).ToListAsync();
                if (passbookDto == null || passbookDto.Count() == 0)
                {
                    messages.Message = "No record found";
                    result.messages.Add(messages);
                }
                else
                {
                    var passbook = passbookDto.Select(p => new PassBookReport(p)).OrderByDescending(x => x.Date).ToList();
                    result.Object = passbook;
                    result.valid = true;
                }

            }
            return Ok(result);
        }
    }
}
