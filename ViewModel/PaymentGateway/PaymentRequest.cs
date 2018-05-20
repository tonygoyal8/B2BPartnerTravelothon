using B2BPartnerTravelothon.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.PaymentGateway
{
    public class PaymentRequest
    {
        public PaymentRequest()
        {
                    
        }
        public string buyer_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string amount { get; set; }
        public string purpose { get; set; }
        public string redirect_url { get; set; }
        public string webhook { get; set; }
        //public ServiceType serviceType { get; set; }
        }
      
}