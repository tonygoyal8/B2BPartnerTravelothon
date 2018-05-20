using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class AvailabilityInputModel
    {
        public AvailabilityInputModel()
        {
          //  JourneyDetails = Enumerable.Empty<TripDetails>();
        }

        public string BookingType { get; set; }
        public string TravelType { get; set; }

        public string ClassType { get; set; }

        public string AirlineCode { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
        public int ResidentofIndia { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Departure { get; set; }
        public string Return { get; set; }

    }
     
}