using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.Bank
{
    [Table("PBanks")]
    public class PBanksDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
        public string UserId { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }

    }
}