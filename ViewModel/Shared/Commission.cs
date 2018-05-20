using B2BPartnerTravelothon.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Shared
{
    public class RechargeCommission
    {
       public int Id { get; set; }
        public string PartnerEmailId { get; set; }
        public string OperatorTransactionId { get; set; }
        public string UserTrackId { get; set; }
        public string ReferenceNo { get; set; }
        public int ApiId { get; set; }
        public int Status { get; set; }
        public ServiceType ServiceType { get; set; }
        public decimal commission { get; set; }
    }
}