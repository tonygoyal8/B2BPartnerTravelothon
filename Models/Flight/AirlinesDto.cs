using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Flight
{
    [Table("Airlines")]
    public class AirlinesDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }

    }
}