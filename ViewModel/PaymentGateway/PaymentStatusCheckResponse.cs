using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.PaymentGateway
{
    public class PaymentStatusCheckResponse
    {
        public int TransactionId { get; set; }
        public int ServiceType { get; set; }
    }
}