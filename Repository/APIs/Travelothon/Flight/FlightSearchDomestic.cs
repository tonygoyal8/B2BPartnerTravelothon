
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using B2BPartnerTravelothon.Domestic_Airlines;
using B2BPartnerTravelothon.ViewModel.DomesticAirlines;
using B2BPartnerTravelothon.ViewModel.Shared;
using B2BPartnerTravelothon.Services;
using Newtonsoft.Json;
using B2BPartnerTravelothon.Repository.Email;
using Microsoft.AspNet.Identity;
using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models.Flight;
using B2BPartnerTravelothon.Models;
using System.Data.Entity;
using B2BPartnerTravelothon.Procedures;
using B2BPartnerTravelothon.Models.Helper;
using B2BPartnerTravelothon.ViewModel.Reports;

namespace OnlineRecharge.Repository.Flight
{
    public class FlightSearchDomestic
    {
        public B2BPartnerTravelothon.Domestic_Airlines.Authentication auth;
        double UDF = 0, PSF = 0, GST = 0, Oth = 0;

        public FlightSearchDomestic()
        {
            auth = new B2BPartnerTravelothon.Domestic_Airlines.Authentication()
            {
                LoginId = ConfigurationManager.AppSettings["PartnerLoginId"].ToString(),
                Password = ConfigurationManager.AppSettings["PartnerPassword"].ToString()
            };
        }


        #region Domestic Flights Tax Calculations
        public async Task<ObjectDto<TaxOutputResponse>> GetDomesticTax(B2BPartnerTravelothon.ViewModel.DomesticAirlines.TaxInput model)
        {
            ObjectDto<TaxOutputResponse> result = new ObjectDto<TaxOutputResponse>();
            var objectDto = new TaxOutputResponse();
            double onewayAmt = 0;
            double onewayBasicAmt = 0;
            double onewaymarkup = 0;
            double onewayCommission = 0;
            double returnAmt = 0;
            double returnBasicAmt = 0;
            double returnmarkup = 0;
            double returnCommission = 0;
            double fee = 0;
            double feePercentage = 0;
            var OperatorsMapping = new List<OperatorMappingHelper>();
            double? adult = 0;
            double? infant = 0;
            double? child = 0;
            var SplitAirlineCode = model.taxReqFlightSegments.FirstOrDefault().AirlineCode.Split(',');
            var SplitClassCode = model.taxReqFlightSegments.FirstOrDefault().ClassCode.Split(',');
            var SplitFlightId = model.taxReqFlightSegments.FirstOrDefault().FlightId.Split(',');
            var SplitSupplierId = model.taxReqFlightSegments.FirstOrDefault().SupplierId.Split(',');
            var returnAirlineCode = "";
            using (var con = new ApplicationDbContext())
            {
                var UId = HttpContext.Current.User.Identity.GetUserId();
                OperatorsMapping = con.SP_B2B_Agent_Details(UId).ToList();
            }
            var prevFlightId = "";
            for (var i = 0; i < SplitFlightId.Count(); i++)
            {
                if (SplitFlightId[i] != prevFlightId)
                {
                    onewaymarkup += Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == SplitAirlineCode[i]) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == SplitAirlineCode[i]).Markup : 0);
                }
                prevFlightId = SplitFlightId[i];
            }
            var lastAirline = SplitAirlineCode[SplitAirlineCode.Count() - 1];
            fee = Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline).Fee : 0);
            feePercentage = Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline).FeePercent : 0);


            var taxSegments = new List<TaxSegments>();
            for (var i = 0; i < SplitAirlineCode.Count(); i++)
            {
                var x = new TaxSegments()
                {
                    AirlineCode = SplitAirlineCode[i],
                    ClassCode = SplitClassCode[i],
                    FlightId = SplitFlightId[i],
                    BasicAmount = 0,
                    ETicketFlag = 1,
                    SupplierId = SplitSupplierId[i]
                };
                taxSegments.Add(x);
            }
            taxSegments[taxSegments.Count() - 1].BasicAmount = Convert.ToDouble(model.taxReqFlightSegments.FirstOrDefault().BasicAmount);
            try
            {
                var tax = new TaxRequest()
                {
                    Authentication = auth,
                    UserTrackId = model.UserTrackId,
                    TaxInput = new B2BPartnerTravelothon.Domestic_Airlines.TaxInput()
                    {
                        TaxReqFlightSegments = taxSegments.ToArray(),
                        GSTDetails = model.gstDetails == null ? null : new B2BPartnerTravelothon.Domestic_Airlines.GSTDetails()
                        {
                            Address = model.gstDetails.Address,
                            GSTNumber = model.gstDetails.GSTNumber,
                            CompanyName = model.gstDetails.CompanyName,
                            ContactNumber = model.gstDetails.ContactNumber,
                            EMailId = model.gstDetails.EMailId,
                            FirstName = model.gstDetails.FirstName,
                            LastName = model.gstDetails.LastName,
                        }
                    }
                };
                var travelothonDomestic = new ApiTravelothon();
                var taxResponse = JsonConvert.DeserializeObject<TaxResponse>(await travelothonDomestic.TravelothonService(
                    JsonConvert.SerializeObject(tax), "DomesticFlight", "GetTax", "POST"));

                if (taxResponse.ResponseStatus == 1)
                {
                    UDF = 0; PSF = 0; GST = 0; Oth = 0;
                    double? adultBasic = 0, childBasic = 0, infantBasic = 0;
                    #region TotalFareCalculations
                    var taxObj = taxResponse.TaxOutput.TaxResFlightSegments[0];
                    adult = adult + taxObj?.AdultTax?.FareBreakUpDetails?.GrossAmount;
                    child = child + taxObj?.ChildTax?.FareBreakUpDetails?.GrossAmount;
                    infant = infant + taxObj?.InfantTax?.FareBreakUpDetails?.GrossAmount;

                    adultBasic += taxObj?.AdultTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.AdultCount);
                    childBasic += taxObj?.ChildTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.ChildCount);
                    infantBasic += taxObj?.InfantTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.InfantCount);



                    var TotalAdultFare = Convert.ToDouble(model.AdultCount) * (adult.HasValue ? adult.Value : 0);
                    var TotalChildFare = Convert.ToDouble(model.ChildCount) * (child.HasValue ? child.Value : 0);
                    var TotalInfantFare = Convert.ToDouble(model.InfantCount) * (infant.HasValue ? infant.Value : 0);
                    onewayAmt = TotalAdultFare + TotalChildFare + TotalInfantFare;
                    onewayBasicAmt = (adultBasic.HasValue ? adultBasic.Value : 0) + (childBasic.HasValue ? childBasic.Value : 0) + (infant.HasValue ? infant.Value : 0);
                    onewayCommission = ((feePercentage / 100) * onewayAmt) + fee;
                    #endregion

                    if (model.TripType.ToString() == "R")
                    {
                        adultBasic = 0; childBasic = 0; infantBasic = 0;
                        var SplitAirlineCodeReturn = model.taxReqFlightSegments.LastOrDefault().AirlineCode.Split(',');
                        var SplitClassCodeReturn = model.taxReqFlightSegments.LastOrDefault().ClassCode.Split(',');
                        var SplitFlightIdReturn = model.taxReqFlightSegments.LastOrDefault().FlightId.Split(',');
                        var SplitSupplierIdReturn = model.taxReqFlightSegments.LastOrDefault().SupplierId.Split(',');
                        returnAirlineCode = SplitAirlineCodeReturn[0];
                        prevFlightId = "";
                        for (var i = 0; i < SplitFlightIdReturn.Count(); i++)
                        {
                            if (SplitFlightIdReturn[i] != prevFlightId)
                            {
                                returnmarkup += Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == SplitAirlineCodeReturn[i]) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == SplitAirlineCodeReturn[i]).Markup : 0);
                            }
                            prevFlightId = SplitFlightIdReturn[i];
                        }
                        lastAirline = SplitAirlineCodeReturn[SplitAirlineCodeReturn.Count() - 1];
                        fee = Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline).Fee : 0);
                        feePercentage = Convert.ToDouble(OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline) != null ? OperatorsMapping.FirstOrDefault(p => p.AirlineCode == lastAirline).FeePercent : 0);


                        var taxSegmentsRetrun = new List<TaxSegments>();
                        for (var i = 0; i < SplitAirlineCodeReturn.Count(); i++)
                        {
                            var x = new TaxSegments()
                            {
                                AirlineCode = SplitAirlineCodeReturn[i],
                                ClassCode = SplitClassCodeReturn[i],
                                FlightId = SplitFlightIdReturn[i],
                                BasicAmount = 0,
                                ETicketFlag = 1,
                                SupplierId = SplitSupplierIdReturn[i]
                            };
                            taxSegmentsRetrun.Add(x);
                        }
                        taxSegmentsRetrun[taxSegmentsRetrun.Count() - 1].BasicAmount = Convert.ToDouble(model.taxReqFlightSegments.LastOrDefault().BasicAmount);
                        tax = new TaxRequest()
                        {
                            Authentication = auth,
                            UserTrackId = model.UserTrackId,
                            TaxInput = new B2BPartnerTravelothon.Domestic_Airlines.TaxInput()
                            {
                                TaxReqFlightSegments = taxSegmentsRetrun.ToArray(),
                                GSTDetails = model.gstDetails == null ? null : new B2BPartnerTravelothon.Domestic_Airlines.GSTDetails()
                                {
                                    Address = model.gstDetails.Address,
                                    GSTNumber = model.gstDetails.GSTNumber,
                                    CompanyName = model.gstDetails.CompanyName,
                                    ContactNumber = model.gstDetails.ContactNumber,
                                    EMailId = model.gstDetails.EMailId,
                                    FirstName = model.gstDetails.FirstName,
                                    LastName = model.gstDetails.LastName,
                                }
                            }

                        };
                        taxResponse = JsonConvert.DeserializeObject<TaxResponse>(await travelothonDomestic.TravelothonService(
                    JsonConvert.SerializeObject(tax), "DomesticFlight", "GetTax", "POST"));
                        if (taxResponse.ResponseStatus == 1)
                        {
                            adult = child = infant = 0;
                            taxObj = taxResponse.TaxOutput.TaxResFlightSegments[0];
                            adult = adult + taxObj?.AdultTax?.FareBreakUpDetails?.GrossAmount;
                            child = child + taxObj?.ChildTax?.FareBreakUpDetails?.GrossAmount;
                            infant = infant + taxObj?.InfantTax?.FareBreakUpDetails?.GrossAmount;

                            adultBasic += taxObj?.AdultTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.AdultCount);
                            childBasic += taxObj?.ChildTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.ChildCount);
                            infantBasic += taxObj?.InfantTax?.FareBreakUpDetails?.BasicAmount * Convert.ToDouble(model.InfantCount);

                            TotalAdultFare = Convert.ToDouble(model.AdultCount) * (adult.HasValue ? adult.Value : 0);
                            TotalChildFare = Convert.ToDouble(model.ChildCount) * (child.HasValue ? child.Value : 0);
                            TotalInfantFare = Convert.ToDouble(model.InfantCount) * (infant.HasValue ? infant.Value : 0);
                            returnAmt = TotalAdultFare + TotalChildFare + TotalInfantFare;
                            returnBasicAmt = (adultBasic.HasValue ? adultBasic.Value : 0) + (childBasic.HasValue ? childBasic.Value : 0) + (infant.HasValue ? infant.Value : 0);
                            returnCommission = ((feePercentage / 100) * returnAmt) + fee;

                        }
                        else
                        {
                            var messages = new Messages();
                            messages.Message = taxResponse.FailureRemarks;
                            result.messages.Add(messages);
                            result.Object = null;
                            var emailService = new EMail();
                            var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                            await emailService.SendAsync(new IdentityMessage()
                            {
                                Body = "API:GetTax" + "REQUEST:" + JsonConvert.SerializeObject(tax) + "<br/>RESPONSE:" + JsonConvert.SerializeObject(taxResponse),
                                Subject = "Tax Error Return",
                                Destination = Destination
                            });
                            return result;
                        }
                    }
                    var data = new TaxOutputResponse()
                    {

                        UserTrackId = taxResponse.UserTrackId,
                        taxOutput = new B2BPartnerTravelothon.ViewModel.DomesticAirlines.TaxOutput()
                        {
                            OneWayTripAmount = onewayAmt,
                            OneWayTripBasicAmount = onewayBasicAmt,
                            OneWayTripMarkup = onewaymarkup,
                            OneWayTripCommission = onewayCommission,
                            RoundTripAmount = returnAmt,
                            RoundTripBasicAmount = returnBasicAmt,
                            RoundTripMarkup = returnmarkup,
                            RoundTripCommission = returnCommission
                        }
                    };

                    objectDto = data;
                    result.Object = objectDto;
                    result.valid = true;

                    return result;
                }
                else
                {
                    var messages = new Messages();
                    messages.Message = taxResponse.FailureRemarks;
                    result.messages.Add(messages);
                    result.Object = null;
                    var emailService = new EMail();
                    var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                    await emailService.SendAsync(new IdentityMessage()
                    {
                        Body = "API:GetTax" + "<br/><br/>REQUEST:" + JsonConvert.SerializeObject(tax) + "<br/><br/>RESPONSE:" + JsonConvert.SerializeObject(taxResponse),
                        Subject = "Tax Error",
                        Destination = Destination
                    });
                    return result;
                }
            }
            catch (Exception e)
            {
                var ex = "\nMessage:" + e.Message + "\n" + e.InnerException + "\n" + e.Data + "\n" + e.GetBaseException();
                var messages = new Messages();
                messages.Message = ex;
                messages.Type = "ERROR";
                result.messages.Add(messages);
                result.Object = null;
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                emailService.SendAsync(new IdentityMessage()
                {
                    Body = ex,
                    Subject = "Tax Error",
                    Destination = Destination
                });
                return result;
            }
        }
        #endregion

        #region Flight Booking
        public async Task<ObjectDto<decimal>> GetDomesticBooking(FlightBookInput model)
        {
            ObjectDto<decimal> result = new ObjectDto<decimal>();
            var UId = HttpContext.Current.User.Identity.GetUserId();
            var basicAmt = model.OneWayTripBasicAmount;
            var amt = model.OneWayTripAmount;
            var markup = model.OneWayTripMarkup;
            var comm = model.OneWayTripCommission;
            var doj = model.DOJ.FirstOrDefault();
            var fareType = model.FareType.FirstOrDefault();
            decimal bal = 0;
            var segType = "O";
            IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
            using (var con = new ApplicationDbContext())
            {
                var userObj = await con.PUserProfile.FindAsync(UId);
                var TraxId = 0;
                var totalAmt = Convert.ToDecimal(model.OneWayTripAmount + model.OneWayTripMarkup + model.RoundTripAmount + model.RoundTripMarkup);
                try
                {
                    if (userObj.Balance > totalAmt)
                    {
                        userObj.Balance -= totalAmt;
                        userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        con.Entry(userObj).State = EntityState.Modified;
                        if (await con.SaveChangesAsync() > 0)
                        {
                            bal = userObj.Balance;
                        }
                        for (var i = 0; i < model.DOJ.Count(); i++)
                        {
                            var flight = new PFlightDto
                            {
                                Adult = model.AdultCount,
                                Child = model.ChildCount,
                                Infant = model.InfantCount,
                                CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                Origin = model.Origin,
                                Destination = model.Destination,
                                APIId = (int)APIId.Hermes,
                                BasicAmount = Convert.ToDecimal(basicAmt),
                                GrossAmount = Convert.ToDecimal(amt),
                                Commission = Convert.ToDecimal(comm),
                                Markup = Convert.ToDecimal(markup),
                                ServiceType = (int)ServiceType.Flight_Domestic,
                                Contact = model.customerDetails.ContactNumber,
                                Status = (int)StatusFlag.Pending,
                                Email = model.customerDetails.EmailId,
                                UserId = UId,
                                DOJ = Convert.ToDateTime(doj),
                                FareType = fareType,
                                Trip = model.Trip,
                                Type = model.BookingType,
                            };
                            con.PFlights.Add(flight);
                            if (await con.SaveChangesAsync() > 0)
                            {
                                if (TraxId == 0)
                                {
                                    TraxId = flight.Id;
                                }
                                foreach (var pax in model.paxItems)
                                {
                                    var detail = new PFlightDetailsDto
                                    {
                                        PFlightId = flight.Id,
                                        Age = Convert.ToInt32(pax.Age),
                                        FrequentFlyerNumber = 0,
                                        Status = (int)StatusFlag.Pending,
                                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                        FirstName = pax.FirstName,
                                        LastName = pax.LastName,
                                        Title = pax.Title,
                                        Type = pax.PassengerType
                                    };
                                    con.PFlightDetails.Add(detail);
                                    await con.SaveChangesAsync();
                                    foreach (var seg in model.flightSegments.Where(s => s.SegmentType == segType))
                                    {
                                        var segments = new PFlightSegmentsDto
                                        {
                                            PFlightDetailsId = detail.Id,
                                            CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                            LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                                            AirlineCode = seg.AirlineCode,
                                            ArrivalDatetime = seg.ArrivalDatetime,
                                            ClassCode = seg.ClassCode,
                                            DepartureDateTime = seg.DepartureDateTime,
                                            Origin = seg.Origin,
                                            Destination = seg.Destination,
                                            FlightNumber = seg.FlightId
                                        };
                                        con.PFlightSegments.Add(segments);
                                    }
                                    await con.SaveChangesAsync();
                                }
                            }

                            var it = model.Origin;
                            model.Origin = model.Destination;
                            model.Destination = it;
                            basicAmt = model.RoundTripBasicAmount;
                            amt = model.RoundTripAmount;
                            markup = model.RoundTripMarkup;
                            comm = model.RoundTripCommission;
                            doj = model.DOJ.LastOrDefault();
                            fareType = model.FareType.LastOrDefault();
                            segType = "R";
                        }
                        result.valid = true;
                        var message = new Messages();
                        message.Message = "*Your booking is in progress. Please check after sometime.";
                        message.Type = Toaster.SUCCESS.ToString();
                        result.Object = bal;
                        result.messages.Add(message);

                        var emailService = new EMail();
                        var flightEmails = Convert.ToString(ConfigurationManager.AppSettings.Get("FlightRequestEmails"));
                        var Destination = flightEmails.Split(';')[0];
                        var BCC = flightEmails.Split(';')[1];
                        var subject = "Flight Booking Request -" + userObj.Agency + " - " + (ReferenceIdHelper.getRefId(ServiceType.Flight_Domestic) + TraxId);
                        var body = "New request has been generated by " + userObj.Agency + " for ₹" + (model.OneWayTripAmount + model.RoundTripAmount)+".";
                        body += "<br/><br/><a href='"+model.Url+"'><b>View Request</b></a>";
                        await emailService.SendAsync(new IdentityMessage()
                        {
                            Body = body,
                            Subject = subject,
                            Destination = Destination
                        },
                        "",
                        BCC
                        );
                    }
                    else
                    {
                        result.valid = false;
                        var message = new Messages();
                        message.Message = "*Your account balance is low";
                        result.Object = bal;
                        result.messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    result.valid = false;
                    var message = new Messages();
                    message.Message = e.InnerException + " " + e.Message;
                    result.Object = bal;
                    result.messages.Add(message);
                }
            }

            return result;

        }
        #endregion

        public async Task<FlightHistoryViewModel> ViewSummary(int id)
        {
            var data = new FlightHistoryViewModel();
            using (var context = new ApplicationDbContext())
            {
                var head = await context.PFlights.FindAsync(id);
                var segs = new List<FlightSegmentReport>();
                var bookingLines = new List<PFlightDetailsDto>();
                var airports = context.Airports.ToList();
                var airlines = await context.Operators.Where(x => x.ServiceType == (int)ServiceType.Flight_Domestic).ToListAsync();
                var h = id;
                var PNR = head.PNR;
                var basicAmt = head.BasicAmount; 
                 var headerId = head.Id;
                using (var con = new ApplicationDbContext())
                {
                    bookingLines = con.PFlightDetails.Where(p => p.PFlightId == headerId).ToList();
                    var booking = bookingLines.FirstOrDefault();
                    var segment = con.PFlightSegments.Where(p => p.PFlightDetailsId == booking.Id);

                    foreach (var s in segment)
                    {
                        var origin = s.Origin.Trim();
                        var destination = s.Destination.Trim();
                        var airlineCode = s.AirlineCode.Trim();
                        airlineCode = airlines.FirstOrDefault(x => x.OperatorCode == airlineCode) == null ? airlineCode : airlines.FirstOrDefault(x => x.OperatorCode == airlineCode).Logo;
                        origin = airports.FirstOrDefault(x => x.IATACode == origin) == null ? origin : airports.FirstOrDefault(x => x.IATACode == origin).Name;
                        destination = airports.FirstOrDefault(x => x.IATACode == destination) == null ? destination : airports.FirstOrDefault(x => x.IATACode == destination).Name;

                        var segReport = new FlightSegmentReport()
                        {
                            AirlineCode = s.AirlineCode,
                            ArrivalDatetime = s.ArrivalDatetime,
                            ClassCode = s.ClassCode,
                            ClassCodeDesc = s.ClassCodeDesc,
                            DepartureDateTime = s.DepartureDateTime,
                            Destination = destination,
                            FlightNumber = s.FlightNumber,
                            Logo = airlineCode,
                            Origin = origin
                        };

                        segs.Add(segReport);
                    }
                }
                return new FlightHistoryViewModel
                {
                    flight = head,
                    flightDetails = bookingLines.ToList(),
                    flightSegments = segs.ToList()

                };
            }
        }

    }
}








