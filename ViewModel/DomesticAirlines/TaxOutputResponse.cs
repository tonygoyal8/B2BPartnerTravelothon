using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.DomesticAirlines
{
    public class TaxOutputResponse
    {
        public string UserTrackId { get; set; }
        public TaxOutput taxOutput { get; set; }
    }
    public class TaxOutput
    {
     //   public int BaggageAndMealsFlag { get; set; }
        public double OneWayTripAmount { get; set; }
        public double OneWayTripBasicAmount { get; set; }
        public double OneWayTripCommission { get; set; }
        public double OneWayTripMarkup { get; set; }

        public double RoundTripBasicAmount { get; set; }
        public double RoundTripAmount { get; set; }
        public double RoundTripCommission { get; set; }
        public double RoundTripMarkup { get; set; }

    }
    public class TaxSegmentDetailsModel
    {
        public string FlightId { get; set; }
        public FareBreakUpDetails AdultTax { get; set; }
        public FareBreakUpDetails ChildTax { get; set; }
        public FareBreakUpDetails InfantTax { get; set; }

    }
    public class FareBreakUpDetails
    {
        public string BasicCurrencyCode { get; set; }
        public string CurrencyCode { get; set; }
        public double? BasicAmount { get; set; }
        public double? EquivalentFare { get; set; }
        public List<TaxItemModel> TaxDetails { get; set; }
        public double? TotalTaxAmount { get; set; }
        public double? TransactionFee { get; set; }
        public double? ServiceCharge { get; set; }
        public double? GrossAmount { get; set; }
        public string Commission { get; set; }
    }
    public class TaxItemModel
    {
        public string Description { get; set; }
        public double Amount { get; set; }
    }
}
