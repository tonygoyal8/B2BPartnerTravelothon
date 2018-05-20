using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.SqlView
{
    public class PassBookView
    {
        public string OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string OperatorDescription { get; set; }
        public string ReferenceNumber { get; set; }
        public int? PointsEarned { get; set; }
        public string UserTrackId {get;set;}
    }
}