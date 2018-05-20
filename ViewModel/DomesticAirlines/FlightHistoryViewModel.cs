//using OnlineRecharge.Models;

using B2BPartnerTravelothon.Models.Flight;
using B2BPartnerTravelothon.ViewModel.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class FlightHistoryViewModel
    {
        public FlightHistoryViewModel()
        {
            flightSegments = Enumerable.Empty<FlightSegmentReport>();
            flightDetails = Enumerable.Empty<PFlightDetailsDto>();
            flight = new PFlightDto();
        }

        public PFlightDto flight { get; set; }
        public IEnumerable<PFlightDetailsDto> flightDetails { get; set; }
        public IEnumerable<FlightSegmentReport> flightSegments { get; set; }

    }
}
