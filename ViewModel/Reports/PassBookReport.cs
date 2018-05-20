using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models.Passbook;
using B2BPartnerTravelothon.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Reports
{
    public class PassBookReport
    {
        public PassBookReport()
        {

        }
        public PassBookReport(PassBookDto model)
        {
            Date = model.CreatedDate;
            RefernceNo = (ReferenceIdHelper.getRefId((ServiceType)model.ServiceId)) + model.TransactionId;
            Amount = model.Amount.HasValue?model.Amount.Value:0;
            Debit = model.Debit.HasValue ? model.Debit.Value : 0;
            Credit = model.Credit.HasValue ? model.Credit.Value : 0;
            Balance = model.Balance.HasValue ? model.Balance.Value : 0;
            Remarks = model.Remarks;
        }
      
        public DateTime Date { get; set; }
        public string RefernceNo  { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string Remarks { get; set; }

    }
}