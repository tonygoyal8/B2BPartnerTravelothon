using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Operator;
using B2BPartnerTravelothon.ViewModel.Shared;
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
    public class MasterDataController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [ResponseType(typeof(ObjectDto<List<OperatorsDto>>))]
        public async Task<IHttpActionResult> GetFlightOperators()
        {
            var messages = new Messages();
            ObjectDto<List<OperatorsDto>> result = new ObjectDto<List<OperatorsDto>>();
            var fOperators = await db.Operators.Where(x => x.ServiceType == (int)ServiceType.Flight_Domestic).ToListAsync();
            if (fOperators == null || fOperators.Count() == 0)
            {
                messages.Message = "No record found";
                result.messages.Add(messages);
            }
            else
            {
                result.valid = true;
                result.Object = fOperators;
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

    }
}
