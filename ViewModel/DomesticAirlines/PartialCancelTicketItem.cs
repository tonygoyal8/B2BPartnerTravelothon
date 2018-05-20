using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class GetCancellationResult {
        public GetCancellationResult()
        {
            CancellationOutputModelMaster = new CancellationDetailsModel();
        }
        public int ResponseStatus { get; set; }
        public CancellationDetailsModel CancellationOutputModelMaster { get; set; }
    }

    public class CancellationDetailsModel
    {
        public CancellationDetailsModel()
        {
            PartialCancelPNRDetailsModelMaster = new PartialCancelPNRDetailsModel();
        }
        public PartialCancelPNRDetailsModel PartialCancelPNRDetailsModelMaster { get; set; }
    }
    public class PartialCancelPNRDetailsModel {
        public PartialCancelPNRDetailsModel()
        {
            CancelPassengerDetailsModelList = new List<CancelPassengerDetailsModel>();
        }
        public decimal TravelothonCancellationCharges { get; set; }
        public decimal AirlineCancellationCharges { get; set; }
        public string AirlineLogo{ get; set; }

        public string HermesPNR { get; set; }
        public string AirlinePNR { get; set; }
        public string CRSPNR { get; set; }
        public string AirlineCode { get; set; }
        public string AirlineName { get; set; }
        public List<CancelPassengerDetailsModel> CancelPassengerDetailsModelList { get; set; }

    }
    public class CancelPassengerDetailsModel
    {
        public CancelPassengerDetailsModel()
        {
            CancelTicketDetailsModelList = new List<CancelPaxItemModel>();
        }
        public int PaxNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PassengerType { get; set; }
        
        public int Status { get; set; }
        public List<CancelPaxItemModel> CancelTicketDetailsModelList { get; set; }
    }
    public class CancelPaxItemModel
    {
        public string TicketNumber { get; set; }
        public int SegmentId { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string ClassCodeDesc { get; set; }
        public double BasicAmount { get; set; }
        public double TotalTaxAmount  { get; set; }
        public double GrossAmount { get; set; }
        public string TicketStatus { get; set; }
        public bool isLive { get; set; }

    }
}