using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class FlightCommRequest
    {
        public Domestic_Airlines.Authentication authentication { get; set; }
        public string UserTrackId { get; set; }
        public string HermesPNR { get; set; }
    }
    public class FlightCommResponse
    {
        public string UserTrackId { get; set; }
        public decimal Commission { get; set; }
        public int ResponseStatus { get; set; }
        public string FailureRemarks { get; set; }
    }
}