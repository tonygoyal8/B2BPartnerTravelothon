using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Reports
{
    public class FlightSegmentReport
    {
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDatetime { get; set; }
        public string AirlineCode { get; set; }
        public string ClassCode { get; set; }
        public string ClassCodeDesc { get; set; }
        public string  Logo { get; set; }
    }
}