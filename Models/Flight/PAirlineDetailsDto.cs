using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Flight
{
    [Table("TAirlineDetails")]
    public class PAirlineDetailsDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int TFlightId { get; set; }
        public string Airline { get; set; }
        public string AirlineCode { get; set; }
        public string PNR { get; set; }

    }
}