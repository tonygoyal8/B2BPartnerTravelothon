using B2BPartnerTravelothon.Domestic_Airlines;
using B2BPartnerTravelothon.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;

namespace B2BPartnerTravelothon.Controllers
{
    public class TransactionStatusController : ApiController
    {
        private Authentication auth;
        public TransactionStatusController()
        {
            auth = new Authentication()
            {
                LoginId = ConfigurationManager.AppSettings["PartnerLoginId"].ToString(),
                Password = ConfigurationManager.AppSettings["PartnerPassword"].ToString()
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> GetTransactionStatus(Authentication authentication)
        {
            try
            {
                await FlightTransactionStatus();
            }
            catch (Exception e) { }
            return Ok();
        }

        [Authorize]
        public async Task<IHttpActionResult> GetUserPoints()
        {
            try
            {
                return Ok(await getPoints());
            }
            catch(Exception e)
            {
                return Ok(e.Message+" "+e.GetBaseException());
            }
        }

        private async Task<decimal> getPoints()
        {
            decimal points = 0;
         
                using (var context = new ApplicationDbContext())
                {
                    var userId = User.Identity.GetUserId();
                    //var user = await context.PUserProfile.FirstOrDefaultAsync(x => x.UserId == userId);
                    //points = user.Points;
                }
           
            return points;
        }



       

   

        private async Task FlightTransactionStatus()
        {
            //try
            //{
            //    using (var context = new ApplicationDbContext())
            //    {
            //        var userId = User.Identity.GetUserId();
            //        auth.LoginId = ConfigurationManager.AppSettings["TravelothonLoginId"].ToString();
            //        var pendingBooking = context.PFlights.FirstOrDefault(p => p.UserId == userId && p.Status == (int)StatusFlag.Pending && p.ServiceType==(int)ServiceType.Flight_Domestic);
            //        if (pendingBooking!=null) {
            //            var bookingHeader = context.PFlights.Where(p => p.UserId == userId && p.UserTrackId== pendingBooking.UserTrackId).ToArray();
            //            if (bookingHeader != null)
            //            {
            //                var pendingTrx = new Domestic_Airlines.TransactionStatusRequest
            //                {
            //                    Authentication = new Domestic_Airlines.Authentication
            //                    {
            //                        LoginId = auth.LoginId,
            //                        Password = auth.Password
            //                    },
            //                    UserTrackId = bookingHeader[0].UserTrackId
            //                };
            //                var travelothonDomestic = new ApiTravelothon();
            //                var transactionResponse = JsonConvert.DeserializeObject<B2BPartnerTravelothon.Domestic_Airlines.TransactionStatusResponse>(await travelothonDomestic.TravelothonService(
            //                    JsonConvert.SerializeObject(pendingTrx), "DomesticFlight", "GetTransactionStatus", "POST"));
            //                if (transactionResponse.ResponseStatus == 1)
            //                {
            //                    if (transactionResponse.TransactionStatusOutput.TransactionStatus.Status == 1)
            //                    {
            //                        IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
            //                        #region TransactionStatus
            //                        for (var k = 0; k < transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails.Count(); k++)
            //                        {

            //                            bookingHeader[k].ReferenceNumber = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].TransactionId;
            //                            bookingHeader[k].HermesPnr = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].HermesPNR;
            //                            bookingHeader[k].GSTNumber = transactionResponse.TransactionStatusOutput.TransactionStatus.GSTDetails.GSTNumber;
            //                            bookingHeader[k].Company = transactionResponse.TransactionStatusOutput.TransactionStatus.GSTDetails.CompanyName;
            //                            bookingHeader[k].Origin = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].BaseOrigin;
            //                            bookingHeader[k].Destination = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].BaseDestination;
            //                            bookingHeader[k].Contact = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].CustomerDetails.ContactNumber;
            //                            bookingHeader[k].Email = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].CustomerDetails.EmailId;
            //                            bookingHeader[k].Status = (int)StatusFlag.Success;
            //                            bookingHeader[k].LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //                            context.SaveChanges();
            //                            #region PaxDetail
            //                            for (var i = 0; i < transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails.Count(); i++)
            //                            {
            //                                var bookingLines = new TFlightDetailsDto()
            //                                {
            //                                    TFlightId = bookingHeader[k].Id,
            //                                    FrequentFlyerNumber = k,
            //                                    Age = Convert.ToInt32(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].Age),
            //                                    FirstName = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].FirstName,
            //                                    LastName = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].LastName,
            //                                    Title = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].Title,
            //                                    Type = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].PassengerType,
            //                                    Status = (int)StatusFlag.Success,
            //                                    TicketNumber = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].TicketNumber,
            //                                    CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //                                    LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
            //                                };
            //                                context.TFlightDetails.Add(bookingLines);
            //                                context.SaveChanges();


            //                                #region BookedSegment
            //                                for (var j = 0; j < transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments.Count(); j++)
            //                                {
            //                                    var bookedSegment = new TFlightSegmentsDto()
            //                                    {
            //                                        AirCraftType = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].AirCraftType,
            //                                        AirlineCode = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].AirlineCode,
            //                                        ArrivalDatetime = DateTime.Parse(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].Arrivaldatetime, culture, System.Globalization.DateTimeStyles.AssumeLocal),
            //                                        BasicAmount = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].BasicAmount),
            //                                        TFlightDetailsId = bookingLines.Id,
            //                                        ClassCode = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].ClassCode,
            //                                        ClassCodeDesc = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].ClassCodeDesc,
            //                                        DepartureDateTime = DateTime.Parse(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].DepartureDateTime, culture, System.Globalization.DateTimeStyles.AssumeLocal),
            //                                        Destination = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].Destination,
            //                                        DestinationAirportTerminal = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].DestinationAirport,
            //                                        EquivalentFare = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].EquivalentFare),
            //                                        FlightNumber = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].FlightNumber,
            //                                        TotalAmount = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].GrossAmount),
            //                                        Origin = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].Origin,
            //                                        OriginAirportTerminal = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].OriginAirport,
            //                                        ServiceCharge = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].ServiceCharge),
            //                                        TicketNumber = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].TicketNumber,
            //                                        TotalTaxAmount = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].TotalTaxAmount),
            //                                        TransactionFee = Convert.ToDecimal(transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].PassengerDetails[i].BookedSegments[j].TransactionFee),
            //                                        CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //                                        LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //                                    };
            //                                    context.TFlightSegments.Add(bookedSegment);
            //                                }
            //                                context.SaveChanges();
            //                                #endregion

            //                            }
            //                            #endregion
            //                            #region AirlineDetails
            //                            for (var s = 0; s < transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].AirlineDetails.Count(); s++)
            //                            {
            //                                var airlineItem = new PAirlineDetailsDto()
            //                                {
            //                                    AirlineCode = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].AirlineDetails[s].AirlineCode,
            //                                    Airline = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].AirlineDetails[s].AirlineName,
            //                                    PNR = transactionResponse.TransactionStatusOutput.TransactionStatus.FlightTicketDetails[k].AirlineDetails[s].AirlinePNR,
            //                                    TFlightId = bookingHeader[k].Id,
            //                                    CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            //                                    LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
            //                                };
            //                                context.TAirlineDetails.Add(airlineItem);
            //                            }
            //                            #endregion
            //                            context.SaveChanges();
            //                            var commRequest = new ViewModel.DomesticAirlines.FlightCommRequest
            //                            {
            //                                authentication = new B2BPartnerTravelothon.Domestic_Airlines.Authentication
            //                                {
            //                                    LoginId = auth.LoginId,
            //                                    Password = auth.Password
            //                                },
            //                                HermesPNR = bookingHeader[k].ReferenceNumber,
            //                                UserTrackId = bookingHeader[k].UserTrackId
            //                            };
            //                            var commResponse = JsonConvert.DeserializeObject<ViewModel.DomesticAirlines.FlightCommResponse>(await travelothonDomestic.TravelothonService(
            //                          JsonConvert.SerializeObject(commRequest), "DomesticFlight", "GetFlightCommission", "POST"));
            //                            var flightProc = new TravelothonFlightProc();
            //                            flightProc.FlightProc(bookingHeader[k].Id, commResponse.Commission);
            //                        }
            //                        #endregion
            //                    }
            //                    else
            //                    {
            //                        foreach (var fh in bookingHeader)
            //                        {
            //                            if (transactionResponse.TransactionStatusOutput.TransactionStatus.Status != (int)StatusFlag.Pending)
            //                            {
            //                                foreach (var item in bookingHeader)
            //                                {
            //                                    item.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //                                    item.Status = (int)StatusFlag.Failed;

            //                                    var payId = item.PaymentId;
            //                                    var pay = context.Payments.FirstOrDefault(x => x.Id == payId);
            //                                    if (pay != null)
            //                                    {
            //                                        pay.Status = (int)StatusFlag.Payment_Refund_Initiated;
            //                                        pay.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //                                        context.Entry(pay).State = EntityState.Modified;
            //                                        item.Status = (int)StatusFlag.Payment_Refund_Initiated;
            //                                    }
            //                                    context.Entry(item).State = EntityState.Modified;
            //                                    userId = item.UserId;
            //                                    var user = await context.PUserProfile.FirstOrDefaultAsync(x => x.UserId == userId);
            //                                    if (user != null)
            //                                    {
            //                                        user.Points += Convert.ToDecimal(item.PointsUsed);
            //                                        context.Entry(user).State = EntityState.Modified;
            //                                    }
            //                                }
            //                                await context.SaveChangesAsync();
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    var emailService = new EMail();
            //    var error = e.Message + "\n" + e.GetBaseException() + "\n" + e.InnerException + "\n" + e.Data;

            //    var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
            //    emailService.SendAsync(new IdentityMessage()
            //    {
            //        Body = error,
            //        Subject = "Flight Book(Pending Trx) Error",
            //        Destination = Destination
            //    });
            //}
        }
    }
}
