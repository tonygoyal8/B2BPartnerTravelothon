using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Flight
{
    [Table("PFlight")]
    public class PFlightDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int APIId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Trip { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DOJ { get; set; }
        public decimal BasicAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Markup { get; set; }
        public string FareType { get; set; }
        public int Adult { get; set; }
        public int Child { get; set; }
        public int Infant { get; set; }
        public int Status { get; set; }
        public string ReferenceNumber { get; set; }
        public int ServiceType { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string GSTNumber { get; set; }
        public string Company { get; set; }
        public decimal Commission { get; set; }
        public string PNR { get; set; }
        public decimal GST { get; set; }
        public decimal SC { get; set; }

    }
}