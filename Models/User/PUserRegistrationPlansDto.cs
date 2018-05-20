using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.User
{
    [Table("PUserRegistrationPlans")]
    public class PUserRegistrationPlansDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int UserRoleId { get; set; }
        public string MemberType { get; set; }
        public string PlanType { get; set; }
        public int Fee { get; set; }
        public bool IsActive { get; set; }
        public string Remarks { get; set; }
        public bool Recharge { get; set; }
        public bool Flight { get; set; }
        public bool DMR { get; set; }
        public bool Rail { get; set; }
        public decimal BalanceAmount { get; set; }

    }
}