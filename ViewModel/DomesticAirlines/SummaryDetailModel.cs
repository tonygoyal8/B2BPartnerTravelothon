using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class SummaryDetailModel
    {
        public string AirlineCode { get; set; }
        public string AirlineName { get; set; }
        public string AirlinePNR { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartTime { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNumber { get; set; }
    }
}