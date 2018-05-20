//using OnlineRecharge.Models.Flight;
using B2BPartnerTravelothon.ViewModel.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B2BPartnerTravelothon.Constants;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class AvailabilityOutputModel
    {
        public string UserTrackId { get; set; }
        public AvailableFlightsModel AvailabilityFlightsOutput { get; set; }

    }


    public class AvailableFlightsModel
    {
        public AvailableFlightsModel()
        {
            OngoingFlights = Enumerable.Empty<AvailFlightSegmentsModel>();
        }
        public IEnumerable<AvailFlightSegmentsModel> OngoingFlights { get; set; }
        public IEnumerable<AvailFlightSegmentsModel> ReturnFlights { get; set; }
    }
    public class AvailFlightSegmentsModel
    {
        public AvailFlightSegmentsModel()
        {
            AvailSegments = Enumerable.Empty<AvailSegmentDetailsModel>();
        }
        public IEnumerable<AvailSegmentDetailsModel> AvailSegments { get; set; }

    }
    public class AvailSegmentDetailsModel
    {
        public string FlightId { get; set; }
        public string AirlineCode { get; set; }
        public string FlightNumber { get; set; }
        public string AirCraftType { get; set; }
        public string Origin { get; set; }
        public string OriginAirportTerminal { get; set; }
        public string Destination { get; set; }
        public string DestinationAirportTerminal { get; set; }
        public string DepartureDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string Duration { get; set; }
        public int NumberofStops { get; set; }
        public string Via { get; set; }
        public string CurrencyCode { get; set; }
        public string Currency_Conversion_Rate { get; set; }
        public IEnumerable<PaxFareDetailsModel> AvailPaxFareDetails { get; set; }
        //public AirlineCodeTable AirlineCodeTableMaster { get; set; }
        public string AirlineName { get; set; }
        public string AirlineLogo { get; set; }
        public decimal Markup { get; set; }
        public string SupplierId { get; set; }
        public FlightTypeFlag FlightType { get; set; }
        public AvailSegmentDetailsModel()
        {
            AvailPaxFareDetails = Enumerable.Empty<PaxFareDetailsModel>();

            // AirlineCodeTable = Enumerable.Empty<AirlineCodeTable>();
        }

    }
    public class PaxFareDetailsModel
    {
        public string ClassCode { get; set; }
        public string ClassCodeDesc { get; set; }
        public AdultModel Adult { get; set; }
        public ChildModel Child { get; set; }
        public InfantModel Infant { get; set; }
        public BaggageAllowedModel BaggageAllowed { get; set; }

    }
    public class BaggageAllowedModel
    {
        public string CheckInBaggage { get; set; }
        public string HandBaggage { get; set; }


    }
    public class AdultModel
    {
        public string FareBasis { get; set; }
        public string FareType { get; set; }
        public double BasicAmount { get; set; }
        public double YQ { get; set; }
        public double TotalTaxAmount { get; set; }
        public double GrossAmount { get; set; }
        public string Commission { get; set; }
        public IEnumerable<AvailTaxItemModel> TaxDetails { get; set; }
        public AdultModel()
        {
            TaxDetails = Enumerable.Empty<AvailTaxItemModel>();
        }

    }
    public class ChildModel
    {
        public string FareBasis { get; set; }
        public string FareType { get; set; }
        public double BasicAmount { get; set; }
        public double YQ { get; set; }
        public double TotalTaxAmount { get; set; }
        public double GrossAmount { get; set; }
        public string Commission { get; set; }
        public IEnumerable<AvailTaxItemModel> TaxDetails { get; set; }
        public ChildModel()
        {
            TaxDetails = Enumerable.Empty<AvailTaxItemModel>();
        }

    }
    public class InfantModel
    {
        public string FareBasis { get; set; }
        public string FareType { get; set; }
        public double BasicAmount { get; set; }
        public double YQ { get; set; }
        public double TotalTaxAmount { get; set; }
        public double GrossAmount { get; set; }
        public string Commission { get; set; }
        public IEnumerable<AvailTaxItemModel> TaxDetails { get; set; }
        public InfantModel()
        {
            TaxDetails = Enumerable.Empty<AvailTaxItemModel>();
        }
    }
    public class AvailTaxItemModel
    {
        public string Description { get; set; }
        public double Amount { get; set; }
    }
}