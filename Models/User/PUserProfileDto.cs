using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Models.User
{
    [Table("PUserProfile")]
    public class PUserProfileDto
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        [Key]
        public string UserId { get; set; }
        public string Agency { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string PanCard { get; set; }
        public string GST { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Aadhar { get; set; }
        public decimal Balance { get; set; }
        public int PlanId { get; set; }
        public int ParentId { get; set; }
        public int PaymentId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

    }
}