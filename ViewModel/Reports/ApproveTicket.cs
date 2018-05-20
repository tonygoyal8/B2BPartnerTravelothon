using B2BPartnerTravelothon.Models.Flight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Reports
{
    public class ApproveTicket
    {
        public int Id { get; set; }
        public string PNR { get; set; }
        public IEnumerable<PFlightDetailsDto> flightDetails { get; set; }
        public ApproveTicket()
        {
            flightDetails = Enumerable.Empty<PFlightDetailsDto>();
        }
    }
}