using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.PaymentGateway
{
    public class Webhook
    {
        public string amount { get; set; }
        public string buyer { get; set; }
        public string buyer_name { get; set; }
        public string buyer_phone { get; set; }
        public string currency { get; set; }
        public string fees { get; set; }
        public string longurl { get; set; }
        public string mac  { get; set; }
        public string payment_id { get; set; }
        public string payment_request_id { get; set; }
        public string purpose { get; set; }
        public string shorturl { get; set; }
        public string status { get; set; }

    }
}