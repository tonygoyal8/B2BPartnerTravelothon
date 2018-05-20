using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Flight
{
    [Table("PFlightDetails")]
    public class PFlightDetailsDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int PFlightId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public int Age { get; set; }
        public int FrequentFlyerNumber { get; set; }
        public int Status { get; set; }
        public string TicketNumber { get; set; }
        public string Title { get; set; }
        public string Remarks { get; set; }
    }
}