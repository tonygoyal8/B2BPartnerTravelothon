using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models.Cases;
using B2BPartnerTravelothon.Models.User;
using B2BPartnerTravelothon.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Cases
{
   
    public class UserCasesViewModel
    {
        public UserCasesViewModel()
        {
        }
        public UserCasesViewModel(UserCasesDto model)
        {
            Id = model.Id;
            ReferenceNo = (ReferenceIdHelper.getRefId((ServiceType)model.ServiceTypeId)) + model.TransactionId;
            Date = model.CreatedDate;
            Issue = ((UserCaseReason)model.Issue).ToString().Replace("_", " ");
            Refund = model.Refund;
            Charged = model.Charged;
            Remarks = model.Remarks;
            Status = ((UserCasesStatus)model.Status).ToString();
            Severity = ((Severity)model.Severity).ToString();
        }

        public UserCasesViewModel(UserCasesDto model,PUserProfileDto user)
        {
            Id = model.Id;
            ReferenceNo = (ReferenceIdHelper.getRefId((ServiceType)model.ServiceTypeId)) + model.TransactionId;
            Date = model.CreatedDate;
            Issue = ((UserCaseReason)model.Issue).ToString().Replace("_"," ");
            Refund = model.Refund;
            Charged = model.Charged;
            Remarks = model.Remarks;
            Status = ((UserCasesStatus)model.Status).ToString();
            Severity = ((Severity)model.Severity).ToString();
            Agency = user.Agency;
            PhoneNo = user.PhoneNumber;
            TransactionId = model.TransactionId;
        }
        public int Id { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime Date { get; set; }
        public string Issue { get; set; }
        public string Status { get; set; }
        public decimal Refund { get; set; }
        public string Remarks { get; set; }
        public string Severity { get; set; }
        public decimal Charged { get; set; }
        public string Agency { get; set; }
        public string PhoneNo { get; set; }
        public int TransactionId { get; set; }
    }
}