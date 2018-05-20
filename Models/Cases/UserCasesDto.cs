using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Cases
{
    [Table("PUserCases")]
    public class UserCasesDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string UserId { get; set; }
        public int ServiceTypeId { get; set; }
        public int TransactionId { get; set; }
        public int Issue { get; set; }
        public int Severity { get; set; }
        public string Remarks { get; set; }
        public decimal Refund { get; set; }
        public decimal Charged { get; set; }
        public int Status { get; set; }
    }
}