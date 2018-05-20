using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Operator
{
    [Table("POperatorMapping")]
    public class POperatorMappingDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int OperatorId { get; set; }
        public int DistributorId { get; set; }
        public decimal? DistributorFee { get; set; }
        public decimal? DistributorFeePercent { get; set; }
        public decimal? SilverFee { get; set; }
        public decimal? SilverFeePercent { get; set; }
        public decimal? GoldFee { get; set; }
        public decimal? GoldFeePercent { get; set; }
        public bool IsEditable { get; set; }
        public Boolean IsActive { get; set; }
    }
}