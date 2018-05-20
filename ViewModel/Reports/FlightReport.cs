using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models.Flight;
using B2BPartnerTravelothon.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Reports
{
    public class FlightReport
    {
        public FlightReport()
        {

        }
        public FlightReport(PFlightDto model)
        {
            Id = model.Id;
            CreatedDate = model.CreatedDate;
            Origin = model.Origin;
            Destination = model.Destination;
            Trip = model.Type;
            DOJ = model.DOJ;
            Status = ((StatusFlag)model.Status).ToString();
            Markup = model.Markup;
            GrossAmount = model.GrossAmount;
            Commission = model.Commission;
        }

        public FlightReport(PFlightDto model,PUserProfileDto user)
        {
            Id = model.Id;
            CreatedDate = model.CreatedDate;
            Origin = model.Origin;
            Destination = model.Destination;
            Trip = model.Type;
            DOJ = model.DOJ;
            Status = ((StatusFlag)model.Status).ToString();
            Markup = model.Markup;
            GrossAmount = model.GrossAmount;
            Commission = model.Commission;
            Agency = user.Agency;
            ContactNo = user.PhoneNumber;
            Email = user.Email;
            Address=user.City+", "+user.State;
        }

        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Trip { get; set; }
        public DateTime DOJ { get; set; }
        public string Status { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Commission { get; set; }
        public decimal Markup { get; set; }
        public string Agency { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}