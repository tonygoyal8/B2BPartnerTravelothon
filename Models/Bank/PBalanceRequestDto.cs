using B2BPartnerTravelothon.ViewModel.BalanceRequest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Bank
{
    [Table("PBalanceRequest")]
    public class PBalanceRequestDto
    {
        public PBalanceRequestDto()
        {

        }
        public PBalanceRequestDto(BalanceRequestViewModel model)
        {
            Id = model.Id;
            CreatedDate = model.CreatedDate;
            LastModifiedDate = model.LastModifiedDate;
            Amount = model.Amount;
            AmountApproved = model.AmountApproved;
            Mode = model.Mode;
            PBankId = model.PBankId;
            TransactionId = model.TransactionId;
            Status = model.Status;
            DepositDate = model.DepositDate;
            DepositorsACNo = model.DepositorsACNo;
            DepositorsBank = model.DepositorsBank;
            DepositorsName = model.DepositorsName;
            UserId = model.UserId;
            Remarks = model.Remarks;
            Purpose = model.Purpose;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
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

    }
}