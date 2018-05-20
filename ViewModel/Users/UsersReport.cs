using B2BPartnerTravelothon.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Users
{
    public class UsersReport
    {
        public UsersReport()
        {

        }
        public UsersReport(PUserProfileDto model,string PAgency)
        {
            Name = model.FirstName + " " + model.LastName;
            Agency = model.Agency;
            Mobile = model.PhoneNumber;
            Email = model.Email;
            Balance = model.Balance;
            Parent = PAgency;
        }

        public string Name { get; set; }
        public string Agency { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string Parent { get; set; }

    }
}