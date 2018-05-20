using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Operator
{
    [Table("Operators")]

    public class OperatorsDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int ServiceType { get; set; }
        public bool IsActive { get; set; }
        public int APIId { get; set; }
        [Key]
        public string OperatorCode { get; set; }
        public string OperatorDescription { get; set; }
        public string Logo { get; set; }
    }
}