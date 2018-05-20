using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Passbook
{
    [Table("PPassbook")]
    public class PassBookDto
    {
        public PassBookDto()
        {

        }
      
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string UserId { get; set; }
        public int ServiceId { get; set; }
        public int TransactionId { get; set; }
        public decimal?  Amount { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Balance { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }

    }
}