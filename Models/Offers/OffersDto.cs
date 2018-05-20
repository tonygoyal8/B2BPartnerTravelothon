using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Offers
{
    [Table("Offers")]
    public class OffersDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string PromoCode { get; set; }
        public int Value { get; set; }
        public int PartnerId { get; set; }
        public int ServiceId { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public int MaxUsage { get; set; }

    }
}