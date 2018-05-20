using B2BPartnerTravelothon.Models.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.BalanceRequest
{
    public class BalanceRequestViewModel
    {
        public BalanceRequestViewModel()
        {

        }
        public BalanceRequestViewModel(PBalanceRequestDto model,string Agency,string PBank)
        {
            Id = model.Id;
            CreatedDate = model.CreatedDate;
            LastModifiedDate = model.LastModifiedDate;
            Amount = model.Amount;
            AmountApproved = model.AmountApproved;
            Mode = model.Mode;

            Status = model.Status;
            TransactionId = model.TransactionId;
            UserId = model.UserId;
            Remarks = model.Remarks;
            Purpose = model.Purpose;

            PBankId = model.PBankId;
            PBankName = PBank;
            this.Agency = Agency;

            DepositorsBank = model.DepositorsBank;
            DepositorsName = model.DepositorsName;
            DepositDate = model.DepositDate;
            DepositorsACNo = model.DepositorsACNo;

        }
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountApproved { get; set; }
        public string Mode { get; set; }
        public int PBankId { get; set; }
        public string TransactionId { get; set; }
        public int Status { get; set; }
        public string DepositorsName { get; set; }
        public string DepositorsBank { get; set; }
        public string DepositorsACNo { get; set; }
        public DateTime DepositDate { get; set; }
        public string UserId { get; set; }
        public string Remarks { get; set; }
        public int Purpose { get; set; }
        public string PBankName { get; set; }
        public string Agency { get; set; }
    }
}