using B2BPartnerTravelothon.Areas.HelpPage.ModelDescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class FlightBookInput
    {
        public string Url { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }

        public string BookingType { get; set; }
        public List<string> DOJ { get; set; }
        public List<string> FareType { get; set; }
        public CustomerDetails customerDetails { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }

        public string Trip { get; set; }


        public IEnumerable<FlightSegments> flightSegments { get; set; }
        public IEnumerable<PaxItemModel> paxItems { get; set; }

        public double OneWayTripAmount { get; set; }
        public double OneWayTripBasicAmount { get; set; }
        public double OneWayTripCommission { get; set; }
        public double OneWayTripMarkup { get; set; }

        public double RoundTripBasicAmount { get; set; }
        public double RoundTripAmount { get; set; }
        public double RoundTripCommission { get; set; }
        public double RoundTripMarkup { get; set; }

        public string GSTNumber { get; set; }
        public FlightBookInput()
        {
            flightSegments = Enumerable.Empty<FlightSegments>();
            paxItems = Enumerable.Empty<PaxItemModel>();
            customerDetails = new CustomerDetails();
        }
    }
    public class PaxItemModel
    {
        public string PassengerType { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string DateofBirth { get; set; }

    }

    [ModelName("TravelothonCustomerDetails")]
    public class CustomerDetails
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
        public string ContactNumber { get; set; }
        public string EmailId { get; set; }
        public string PinCode { get; set; }

    }

    public class FlightSegments
    {
        public string FlightId { get; set; }
        public string ClassCode { get; set; }
        public string AirlineCode { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDatetime { get; set; }
        public string SegmentType { get; set; }
    }
}