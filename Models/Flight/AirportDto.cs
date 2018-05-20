using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace  B2BPartnerTravelothon.Models.Flight
{
    [Table("Airports")]
    public class AirportDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string IATACode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

     //   public string AirpotNameAndCode { get { return AirpotName + ", " + AirpotCode; } }
    }
   
}