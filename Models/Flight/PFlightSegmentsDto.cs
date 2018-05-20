using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Flight
{
    [Table("PFlightSegments")]
    public class PFlightSegmentsDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int PFlightDetailsId { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDatetime { get; set; }
        public string AirlineCode { get; set; }
        public string ClassCode { get; set; }
        public string ClassCodeDesc { get; set; }
    }
}