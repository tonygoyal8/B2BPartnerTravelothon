using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Payment
{
    [Table("PPayments")]
    public class PaymentDto
    {
        public PaymentDto(){}
        public PaymentDto(dynamic pay,int status)
        {
            CreatedDate = LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            Purpose = pay.purpose;
            Amount = Convert.ToDecimal(pay.amount);
            Fees = Convert.ToDecimal(pay.fees);
            Status = status;
            PaymentRequestId = pay.id;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Purpose { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Fees { get; set; }
        public string MAC { get; set; }
        public int? Status { get; set; }
        [Key]
        public string PaymentRequestId { get; set; }
        public string PaymentId { get; set; }
    }
}