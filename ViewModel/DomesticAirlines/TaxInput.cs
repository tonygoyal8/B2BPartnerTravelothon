using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B2BPartnerTravelothon.Areas.HelpPage.ModelDescriptions;
using B2BPartnerTravelothon.Constants;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    [ModelName("TravelothonTaxInput")]
    public class TaxInput
    {
        public GSTDetails gstDetails { get; set; }
      public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
        public TripTypeFlag TripType { get; set; }
        public FlightTypeFlag FlightType { get; set; }
        public string UserTrackId { get; set; }
        public IEnumerable<TaxReqFlightSegments> taxReqFlightSegments { get; set; }
        public TaxInput()
        {
            taxReqFlightSegments = Enumerable.Empty<TaxReqFlightSegments>();
        }

    }
    [ModelName("TravelothonGSTDetails")]

    public class GSTDetails
    {
        public string GSTNumber { get; set; }
        public string EMailId { get; set; }
        public string CompanyName { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
    [ModelName("TravelothonTaxReqFlightSegments")]

    public class TaxReqFlightSegments
    {
        public string FlightId { get; set; }
        public string ClassCode { get; set; }
        public string AirlineCode { get; set; }
        public int ETicketFlag { get; set; }
        public Double BasicAmount { get; set; }
        public string SupplierId { get; set; }
    }
}