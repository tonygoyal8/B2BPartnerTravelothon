using B2BPartnerTravelothon.Models.Flight;
using B2BPartnerTravelothon.ViewModel.DomesticAirlines;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using B2BPartnerTravelothon.Domestic_Airlines;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.ViewModel.Shared;
using System.Threading.Tasks;
using System.Data.Entity;
using OnlineRecharge.Repository.Flight;
using B2BPartnerTravelothon.Services;
using Newtonsoft.Json;
using B2BPartnerTravelothon.Repository.Email;
using Microsoft.AspNet.Identity;
using B2BPartnerTravelothon.Constants;
using System.Net.Mail;
using System.Net.Mime;
using B2BPartnerTravelothon.ViewModel.Reports;

namespace B2BPartnerTravelothon.Controllers
{
    //[RoutePrefix("api/travelothonflight")]

    [Authorize(Roles=Roles.TA+","+Roles.AD)]
    public class FlightController : ApiController
    {
        private Domestic_Airlines.Authentication auth;
        private ApplicationDbContext context;

        double ApiBalanceMargin;
        public FlightController()
        {
            context = new ApplicationDbContext();
            auth = new Domestic_Airlines.Authentication()
            {
                LoginId = ConfigurationManager.AppSettings["PartnerLoginId"].ToString(),
                Password = ConfigurationManager.AppSettings["PartnerPassword"].ToString()
            };
            ApiBalanceMargin = Convert.ToDouble(ConfigurationManager.AppSettings["ApiBalanceMargin"]);
        }
        [HttpGet]
        [ResponseType(typeof(ObjectDto<AvailabilityOutputModel>))]
        public async Task<IHttpActionResult> SearchResult([FromUri] AvailabilityInputModel model)
        {
            ObjectDto<AvailabilityOutputModel> result = new ObjectDto<AvailabilityOutputModel>();
            var objectDto = new AvailabilityOutputModel();
            var messages = new Messages();
            try
            {
                var destination = model.Destination.Contains(',') ? model.Destination.Split(',')[1].Trim() : model.Destination;
                var origin = model.Origin.Contains(',') ? model.Origin.Split(',')[1].Trim() : model.Origin;

                #region AvailabilityRequestModel
                var journeyDate = new List<TripDetails>();
                journeyDate.Add(new TripDetails() { Destination = destination.ToUpper(), Origin = origin.ToUpper(), TravelDate = model.Departure });
                if (!String.IsNullOrEmpty(model.Return))
                {
                    journeyDate.Add(new TripDetails() { Destination = origin.ToUpper(), Origin = destination.ToUpper(), TravelDate = model.Return });
                }
                var avial = new AvailabilityInput()
                {
                    AdultCount = Convert.ToUInt16(model.AdultCount),
                    AirlineCode = !String.IsNullOrEmpty(model.AirlineCode) ? model.AirlineCode : "",
                    BookingType = model.BookingType,
                    ChildCount = Convert.ToUInt16(model.ChildCount),
                    ClassType = model.ClassType,
                    InfantCount = Convert.ToUInt16(model.InfantCount),
                    JourneyDetails = journeyDate.ToArray(),
                    Optional1 = "0",
                    Optional2 = "0",
                    Optional3 = "0",
                    ResidentofIndia = 1
                };

                #endregion

                var availabiltRequest = new AvailabilityRequest()
                {
                    AvailabilityInput = avial,
                    Authentication = auth
                };
                //AvailabilityResponse response = await flightSearchDomestic.GetAvailability(availabiltRequest);
                var travelothonDomestic = new ApiTravelothon();
                AvailabilityResponse response = JsonConvert.DeserializeObject<AvailabilityResponse>(await travelothonDomestic.TravelothonService(
                    JsonConvert.SerializeObject(availabiltRequest), "DomesticFlight", "GetAvailability", "POST"));

                if (response.ResponseStatus == 1)
                {
                    var airlines = await context.Operators.Where(x => x.ServiceType == (int)ServiceType.Flight_Domestic).ToListAsync(); 
                    var UId = User.Identity.GetUserId();
                    var OperatorsMapping = context.SP_B2B_Agent_Details(UId).ToList();

                    #region Oneway
                    if (String.Equals(model.BookingType, "O"))
                    {
                        var Finaldata = new AvailabilityOutputModel()
                        {
                            UserTrackId = response.UserTrackId,
                            AvailabilityFlightsOutput = new AvailableFlightsModel()
                            {
                                OngoingFlights = response.AvailabilityOutput.AvailableFlights.OngoingFlights.Select(k => new AvailFlightSegmentsModel()
                                {
                                    AvailSegments = k.AvailSegments.Select(j => new AvailSegmentDetailsModel()
                                    {
                                        SupplierId = j.SupplierId,
                                        AirCraftType = j.AirCraftType,
                                        AirlineCode = j.AirlineCode,
                                        ArrivalDateTime = j.ArrivalDateTime,
                                        CurrencyCode = j.CurrencyCode,
                                        Currency_Conversion_Rate = j.Currency_Conversion_Rate,
                                        DepartureDateTime = j.DepartureDateTime,
                                        NumberofStops = j.NumberofStops,
                                        FlightId = j.FlightId,
                                        Destination = j.Destination,
                                        DestinationAirportTerminal = j.DestinationAirportTerminal,
                                        Duration = j.Duration,
                                        FlightNumber = j.FlightNumber,
                                        Origin = j.Origin,
                                        OriginAirportTerminal = j.OriginAirportTerminal,
                                        Via = j.Via,
                                        AirlineName = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).OperatorDescription,
                                        AirlineLogo = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).Logo,
                                        Markup = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ? OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Markup:0,
                                        #region AvailPaxFareDetails
                                        AvailPaxFareDetails = j.AvailPaxFareDetails.Select(t => new PaxFareDetailsModel()
                                        {
                                            Adult = new AdultModel()
                                            {
                                                BasicAmount = t.Adult.BasicAmount,
                                                Commission = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ?
                                                Convert.ToString(OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Fee +
                                                ((OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).FeePercent / 100) * Convert.ToDecimal(t.Adult.GrossAmount)))
                                                : "0",

                                                FareBasis = t.Adult.FareBasis,
                                                FareType = t.Adult.FareType,
                                                GrossAmount = t.Adult.GrossAmount,
                                                TaxDetails = t.Adult.TaxDetails != null ? t.Adult.TaxDetails.Select(q => new AvailTaxItemModel() { Amount = q.Amount, Description = q.Description }) : Enumerable.Empty<AvailTaxItemModel>(),
                                                TotalTaxAmount = t.Adult.TotalTaxAmount,
                                                YQ = t.Adult.YQ
                                            },
                                            BaggageAllowed = new BaggageAllowedModel()
                                            {
                                                CheckInBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.CheckInBaggage,
                                                HandBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.HandBaggage
                                            },
                                            ClassCode = t.ClassCode,
                                            ClassCodeDesc = t.ClassCodeDesc
                                        })
                                        #endregion
                                    })
                                })
                            }
                        };
                        objectDto = Finaldata;
                    }
                    else
                    {
                        var returnData = new AvailabilityOutputModel()
                        {
                            UserTrackId = response.UserTrackId,
                            AvailabilityFlightsOutput = new AvailableFlightsModel()
                            {
                                OngoingFlights = response.AvailabilityOutput.AvailableFlights.OngoingFlights.Select(k => new AvailFlightSegmentsModel()
                                {
                                    AvailSegments = k.AvailSegments.Select(j => new AvailSegmentDetailsModel()
                                    {
                                        SupplierId = j.SupplierId,
                                        AirCraftType = j.AirCraftType,
                                        AirlineCode = j.AirlineCode,
                                        ArrivalDateTime = j.ArrivalDateTime,
                                        CurrencyCode = j.CurrencyCode,
                                        Currency_Conversion_Rate = j.Currency_Conversion_Rate,
                                        DepartureDateTime = j.DepartureDateTime,
                                        NumberofStops = j.NumberofStops,
                                        FlightId = j.FlightId,
                                        Destination = j.Destination,
                                        DestinationAirportTerminal = j.DestinationAirportTerminal,
                                        Duration = j.Duration,
                                        FlightNumber = j.FlightNumber,
                                        Origin = j.Origin,
                                        OriginAirportTerminal = j.OriginAirportTerminal,
                                        Via = j.Via,
                                        AirlineName = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).OperatorDescription,
                                        AirlineLogo = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).Logo,
                                        Markup = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ? OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Markup:0,
                                        #region AvailPaxFareDetails
                                        AvailPaxFareDetails = j.AvailPaxFareDetails.Select(t => new PaxFareDetailsModel()
                                        {
                                            Adult = new AdultModel()
                                            {
                                                BasicAmount = t.Adult.BasicAmount,
                                                Commission = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ?
                                                Convert.ToString(OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Fee +
                                                ((OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).FeePercent / 100) * Convert.ToDecimal(t.Adult.GrossAmount)))
                                                : "0",
                                                FareBasis = t.Adult.FareBasis,
                                                FareType = t.Adult.FareType,
                                                GrossAmount = t.Adult.GrossAmount,
                                                TaxDetails = t.Adult.TaxDetails != null ? t.Adult.TaxDetails.Select(q => new AvailTaxItemModel() { Amount = q.Amount, Description = q.Description }) : Enumerable.Empty<AvailTaxItemModel>(),
                                                TotalTaxAmount = t.Adult.TotalTaxAmount,
                                                YQ = t.Adult.YQ
                                            },
                                            BaggageAllowed = new BaggageAllowedModel()
                                            {
                                                CheckInBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.CheckInBaggage,
                                                HandBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.HandBaggage
                                            },

                                            ClassCode = t.ClassCode,
                                            ClassCodeDesc = t.ClassCodeDesc
                                        })
                                        #endregion
                                    })
                                }),
                                ReturnFlights = response.AvailabilityOutput.AvailableFlights.ReturnFlights.Select(k => new AvailFlightSegmentsModel()
                                {
                                    AvailSegments = k.AvailSegments.Select(j => new AvailSegmentDetailsModel()
                                    {
                                        SupplierId = j.SupplierId,
                                        AirCraftType = j.AirCraftType,
                                        AirlineCode = j.AirlineCode,
                                        ArrivalDateTime = j.ArrivalDateTime,
                                        CurrencyCode = j.CurrencyCode,
                                        Currency_Conversion_Rate = j.Currency_Conversion_Rate,
                                        DepartureDateTime = j.DepartureDateTime,
                                        NumberofStops = j.NumberofStops,
                                        FlightId = j.FlightId,
                                        Destination = j.Destination,
                                        DestinationAirportTerminal = j.DestinationAirportTerminal,
                                        Duration = j.Duration,
                                        FlightNumber = j.FlightNumber,
                                        Origin = j.Origin,
                                        OriginAirportTerminal = j.OriginAirportTerminal,
                                        Via = j.Via,
                                        AirlineName = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).OperatorDescription,
                                        AirlineLogo = airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode) == null ? "" : airlines.FirstOrDefault(p => p.OperatorCode == j.AirlineCode).Logo,
                                        Markup = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ? OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Markup:0,
                                        //#region AvailPaxFareDetails
                                        AvailPaxFareDetails = j.AvailPaxFareDetails.Select(t => new PaxFareDetailsModel()
                                        {
                                            Adult = new AdultModel()
                                            {
                                                BasicAmount = t.Adult.BasicAmount,
                                                Commission = OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode) != null ?
                                                Convert.ToString(OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).Fee +
                                                ((OperatorsMapping.FirstOrDefault(x => x.AirlineCode == j.AirlineCode).FeePercent / 100) * Convert.ToDecimal(t.Adult.GrossAmount)))
                                                : "0",
                                                FareBasis = t.Adult.FareBasis,
                                                FareType = t.Adult.FareType,
                                                GrossAmount = t.Adult.GrossAmount,
                                                TaxDetails = t.Adult.TaxDetails != null ? t.Adult.TaxDetails.Select(q => new AvailTaxItemModel() { Amount = q.Amount, Description = q.Description }) : Enumerable.Empty<AvailTaxItemModel>(),
                                                TotalTaxAmount = t.Adult.TotalTaxAmount,
                                                YQ = t.Adult.YQ
                                            },
                                            BaggageAllowed = new BaggageAllowedModel()
                                            {
                                                CheckInBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.CheckInBaggage,
                                                HandBaggage = t.BaggageAllowed == null ? "" : t.BaggageAllowed.HandBaggage
                                            },
                                            ClassCode = t.ClassCode,
                                            ClassCodeDesc = t.ClassCodeDesc
                                        })
                                        #endregion
                                    })
                                })
                            }

                        };
                        objectDto = returnData;
                    }
                }
                else
                {
                    messages.Message = response.FailureRemarks;
                    result.messages.Add(messages);
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.InnerException + " " + e.GetBaseException();
                result.messages.Add(messages);
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Flight Search Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
            }
            try
            {
                result.Object = objectDto;
                return Ok(result);
            }
            catch (Exception e)
            {
                return Ok(e.Message + " " + e.GetBaseException());
            }
        }

        [HttpPost]
        [ResponseType(typeof(ObjectDto<TaxOutputResponse>))]
        public async Task<IHttpActionResult> GetTaxDetails(ViewModel.DomesticAirlines.TaxInput model)
        {
            var flightSearchDomestic = new FlightSearchDomestic();
            var objectDto = await flightSearchDomestic.GetDomesticTax(model);
            if (objectDto.valid)
            {
                var UserBal = Convert.ToDouble(await GetBalance());
              //  var bal = await TraxBalanceCheck();
                var amt = objectDto.Object.taxOutput.OneWayTripAmount + objectDto.Object.taxOutput.RoundTripAmount;
                if (UserBal < amt)
                {
                    objectDto.valid = false;
                    objectDto.Object = null;
                    var messages = new Messages();
                    messages.Message = "*Your account balance is low.";
                    messages.Type = Toaster.ERROR.ToString();
                    objectDto.messages.Add(messages);
                    return Ok(objectDto);
                }
            }
            return Ok(objectDto);
        }

        [HttpPost]
        [ResponseType(typeof(ObjectDto<decimal>))]
        public async Task<IHttpActionResult> GetBook(FlightBookInput model)
        {
            var flightSearchDomestic = new FlightSearchDomestic();
            var objectDto = await flightSearchDomestic.GetDomesticBooking(model);
          
            return Ok(objectDto);
        }



        [ResponseType(typeof(ObjectDto<String>))]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetFareRules(string AirlineCode, string FlightId, string ClassCode, string UserTrackId, string SupplierId)
        {
            var messages = new Messages();
            ObjectDto<String> result = new ObjectDto<String>();
            string objectDto = string.Empty;
            try
            {
                var travelothonDomestic = new ApiTravelothon();
                var data = new FareRuleRequest()
                {
                    Authentication = auth,
                    UserTrackId = UserTrackId,
                    FareRuleInput = new FareRuleInput()
                    {
                        AirlineCode = AirlineCode,
                        FlightId = FlightId,
                        ClassCode = ClassCode,
                        SupplierId = SupplierId
                    }
                };

                var fairResponse = JsonConvert.DeserializeObject<FareRuleResponse>(await travelothonDomestic.TravelothonService(
                    JsonConvert.SerializeObject(data), "DomesticFlight", "GetFareRule", "POST"));

                if (fairResponse.ResponseStatus == 1)
                {
                    objectDto = fairResponse.FareRuleOutput.FareRules;
                }
                else
                {
                    messages.Message = fairResponse.FailureRemarks;
                    var emailService = new EMail();
                    var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                    await emailService.SendAsync(new IdentityMessage()
                    {
                        Body = "API:GetFareRule<br/><br/>REQUEST:" + JsonConvert.SerializeObject(data) + "<br/><br/>RESPONSE:" + JsonConvert.SerializeObject(fairResponse),
                        Subject = "GetFareRules Error.Environment " + Request.RequestUri.Host.ToString(),
                        Destination = Destination
                    });
                    result.messages.Add(messages);
                }
            }
            catch (Exception e)
            {
                messages.Message = "Internal Server Error";
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
               await  emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "GetFareRules Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                result.messages.Add(messages);
            }
            result.Object = objectDto;
            return Ok(result);

        }

        [ResponseType(typeof(ObjectDto<List<AirportDto>>))]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAirpotCodes()
        {

            var result = new ObjectDto<List<AirportDto>>();
            var messages = new Messages();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var airport = await context.Airports.ToListAsync();
                    result.Object = airport;
                }
            }
            catch (Exception e)
            {
                messages.Message = e.Message + " " + e.GetBaseException();
                result.messages.Add(messages);
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Airport Error",
                    Destination = Destination
                });
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<FlightHistoryViewModel>))]
        public async Task<IHttpActionResult> GetBookingTicket(int id)
        {
            var result = new ObjectDto<FlightHistoryViewModel>();
            var flightSearchDomestic = new FlightSearchDomestic();
            try
            {
                result.Object = await flightSearchDomestic.ViewSummary(id);
                result.valid = true;
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = "Internal Server Error";
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
               await emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Flight Ticket Error.Environment " + Request.RequestUri.Host.ToString(),
                   Destination = Destination
                });
                result.messages.Add(messages);
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<List<FlightReport>>))]
        public async Task<IHttpActionResult> GetFlights(string startdate,string enddate )
        {
            var result = new ObjectDto<List<FlightReport>>();
            var messages = new Messages();
            var UId = User.Identity.GetUserId();
            if (!String.IsNullOrEmpty(enddate) && !String.IsNullOrEmpty(enddate))
            {
                var StartDate = TimeZoneInfo.ConvertTime(Convert.ToDateTime(startdate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;
                var EndDate = TimeZoneInfo.ConvertTime(Convert.ToDateTime(enddate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date.AddDays(1);
                using (var con = new ApplicationDbContext())
                {
                    var flightsDto = await con.PFlights.Where(x => x.UserId == UId && x.CreatedDate>= StartDate && x.CreatedDate<=EndDate ).ToListAsync();
                    var flights = flightsDto.Select(p => new FlightReport(p)).OrderByDescending(x => x.CreatedDate).ToList();
                    if (flights == null || flights.Count() == 0)
                    {
                        messages.Message = "No record found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.Object = flights;
                        result.valid = true;
                    }
                }
            }
            else
            {
                messages.Message = "Filter Dates are mandatory.";
                result.messages.Add(messages);
            }
                return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<List<FlightReport>>))]
        public async Task<IHttpActionResult> GetFlightRequets(string startdate, string enddate)
        {
            var result = new ObjectDto<List<FlightReport>>();
            var messages = new Messages();
            if (!String.IsNullOrEmpty(enddate) && !String.IsNullOrEmpty(enddate))
            {
                var StartDate = TimeZoneInfo.ConvertTime(Convert.ToDateTime(startdate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;
                var EndDate = TimeZoneInfo.ConvertTime(Convert.ToDateTime(enddate), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date.AddDays(1);
                using (var con = new ApplicationDbContext())
                {
                    //var flightsDto = await con.PFlights.ToListAsync();
                    var flightsDto = (from pf in con.PFlights
                                      join pu in con.PUserProfile  on pf.UserId equals pu.UserId
                                      where pf.CreatedDate >= StartDate && pf.CreatedDate <= EndDate
                                      select new { pu, pf }).ToList();

                    var flights = flightsDto.Select(p => new FlightReport(p.pf, p.pu)).OrderByDescending(x => x.CreatedDate).ToList();
                    if (flights == null || flights.Count() == 0)
                    {
                        messages.Message = "No record found";
                        result.messages.Add(messages);
                    }
                    else
                    {
                        result.Object = flights;
                        result.valid = true;
                    }
                }
            }
            else
            {
                messages.Message = "Filter Dates are mandatory.";
                result.messages.Add(messages);
            }
            return Ok(result);
        }



        [ResponseType(typeof(ObjectDto<string>))]
        public async Task<IHttpActionResult> GetFlightTicketEMail(int id)
        {
            var result = new ObjectDto<string>();
            var flightSearchDomestic = new FlightSearchDomestic();
            try
            {
                var data = new ObjectDto<FlightHistoryViewModel>();
                data.Object =await flightSearchDomestic.ViewSummary(id);
                if (data.Object != null)
                    await emailTicket(data);
                result.valid = true;
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = "Internal Server Error";
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Flight Ticket Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                result.messages.Add(messages);
            }
            return Ok(result);
        }


        [ResponseType(typeof(ObjectDto<string>))]
        [HttpPost]
        public async Task<IHttpActionResult> ApproveTicket(ApproveTicket model)
        {
            ObjectDto<string> result = new ObjectDto<string>();
            var message = new Messages();
            try
            {
                using (var con=new ApplicationDbContext())
                {
                    var flight=await con.PFlights.FindAsync(model.Id);
                    var status = flight.Status;

                    flight.PNR = model.PNR;
                    flight.Status = (int)StatusFlag.Success;
                    flight.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    con.Entry(flight).State = EntityState.Modified;
                    foreach(var detail in model.flightDetails)
                    {
                        detail.Status= (int)StatusFlag.Success;
                        detail.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                        con.Entry(detail).State = EntityState.Modified;
                    }
                    if ((await con.SaveChangesAsync()) > 0)
                    {
                        try
                        {
                            if (status == (int)StatusFlag.Pending)
                            {
                                con.SP_B2B_Passbook(flight.Id, (int)ServiceType.Flight_Domestic);
                            }
                        }
                        catch (Exception e){
                            var d = e.InnerException + " " + e.GetBaseException() + " " + e.Message;
                            var emailService = new EMail();
                            var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                            emailService.SendAsync(new IdentityMessage()
                            {
                                Body =d,
                                Subject = "B2B Pass book Proc Error.Environment " + Request.RequestUri.Host.ToString(),
                                Destination = Destination
                            });
                        }
                        result.valid = true;
                        message.Type = Toaster.SUCCESS.ToString();
                        message.Message = "Ticket has been approved";
                        result.messages.Add(message);
                    }
                    else
                    {
                        message.Message = "Unable to approve the ticket.Please try again after sometime.";
                        result.messages.Add(message);
                    }

                }
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = "Internal Server Error";
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Flight Ticket Approval Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                result.messages.Add(messages);
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<string>))]
        public async Task<IHttpActionResult> DenyTicket(int id)
        {
            ObjectDto<string> result = new ObjectDto<string>();
            var message = new Messages();
            try
            {
                using (var con = new ApplicationDbContext())
                {
                    var flight = await con.PFlights.FindAsync(id);
                    flight.Status = (int)StatusFlag.Failed;
                    flight.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    con.Entry(flight).State = EntityState.Modified;
                    var flightDetails = await con.PFlightDetails.Where(x => x.PFlightId == id).ToListAsync();
                    foreach (var detail in flightDetails)
                    {
                        detail.Status = (int)StatusFlag.Failed;
                        detail.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                       con.Entry(detail).State = EntityState.Modified;
                    }
                    var userObj = await con.PUserProfile.FirstOrDefaultAsync(x => x.UserId == flight.UserId);
                    userObj.Balance += flight.GrossAmount + flight.Markup;
                    userObj.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    con.Entry(userObj).State = EntityState.Modified;

                    if ((await con.SaveChangesAsync()) > 0)
                    {
                        result.valid = true;
                        message.Type = Toaster.SUCCESS.ToString();
                        message.Message = "Ticket has been rejected";
                        result.messages.Add(message);
                    }
                    else
                    {
                        message.Message = "Unable to reject the ticket request.Please try again after sometime.";
                        result.messages.Add(message);
                    }

                }
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = "Internal Server Error";
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Flight Ticket Approval Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
                result.messages.Add(messages);
            }
            return Ok(result);
        }


        #region PrivateMethods
        private async Task emailTicket(ObjectDto<FlightHistoryViewModel> result)
        {
            try
            {
                #region Email Ticket
                //var basicFare = result.Object.flightSegments.Sum(x => x.BasicAmount);
                var airline = result.Object.flightSegments.FirstOrDefault().Logo.Split('.')[0];
                //basicFare *= result.Object.flightSegments.Count();
                LinkedResource LinkedImage = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~/assets/TravelothonEmail/TravelothonEmailLogo.jpg"));
                LinkedImage.ContentType = new ContentType(MediaTypeNames.Image.Jpeg);
                LinkedImage.ContentId = "Travelothon";
                var Body = "<div style='width: 100%;padding-right: 15px;padding-left: 15px;margin-right: auto;margin-left: auto;max-width: 700px;font-family: Verdana;font-size:12px;text-align:left'>Dear " + result.Object.flightDetails.FirstOrDefault().FirstName + " " + result.Object.flightDetails.FirstOrDefault().LastName + ",";
                Body += "<br/><br/><span style='font-size:14px;font-weight:bold;'>Congratulations! Your flight booking has been confirmed.</span></div><br/><br/>";

                var ticket = "<div style='width: 100%;padding-right: 15px;padding-left: 15px;margin-right: auto;margin-left: auto;max-width: 700px;'>";
                ticket += "<div style='position:relative;display: flex;-webkit-box-orient: vertical;-webkit-box-direction: normal;flex-direction: column;min-width: 0;word-wrap: break-word;background-color: #fff;background-clip: border-box;border: 1px solid rgba(0,0,0,.125);border-radius: 0.25rem;padding-left: 1.5em;padding-right: 1.5em;'>";
                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding: 1.25rem;padding-left: 0;padding-right: 0;width: 72em;'>";
                ticket += "<div style='display: flex;flex-wrap: wrap;margin-right: -15px;margin-left: -15px;'>";
                ticket += "<div style='-webkit-box-flex: 0;flex: 0 0 41.666667%;width:67%;'>";
                ticket += "<div style='position: relative;display: flex;-webkit-box-orient: vertical;-webkit-box-direction: normal;flex-direction: column;min-width: 0;word-wrap: break-word;background-color: #fff;background-clip: border-box;float: left;border: 1px groove #000;width: 20em;border-radius: .5em;'>";
                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding: 1rem;padding-left: 0;padding-right: 0;'>";
                ticket += "<span style='font-family: Lato,sans-serif;font-size: 15px;padding: 5em;'>AIRLINE PNR</span><br/><span style='font-family: Lato,sans-serif;font-size: 20px;padding: 4.2em;'>" + result.Object.flight.PNR + "</span></div></div></div>";
                ticket += "<div style='-webkit-box-flex: 0;flex: 0 0 41.666667%;max-width: 41.666667%;'>";
                ticket += "<div style='float: right;'>";
                ticket += "<img src=cid:Travelothon  style='vertical-align: middle;border-style: none;height: 70px;display: block;float: right;' >";
                ticket += "</div></div></div>";

                ticket += "<div style='margin-top: 1em;margin-bottom: -.5em;'>";
                ticket += "<span style='margin-bottom: -.5em;font-weight: 700;font-size: 12px;'>Flight Details</span>";
                ticket += "<span style='float: right;font-size: 12px;'><span style='font-weight: 700;'>Reference ID:</span> " + result.Object.flight.PNR + " | <span style='font-weight: 700;'>Booked On:</span>" + result.Object.flight.CreatedDate.ToLongDateString() + "</span></div>";

                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding:0px;padding-top:0.8em'>";
                ticket += "<table style='border-collapse: collapse;width: 100%;max-width: 100%;margin-bottom: 1rem;background-color: transparent;'>";
                ticket += "<tbody style='display: table-row-group;vertical-align: middle;border-color: inherit;'>";
                ticket += "<tr style='background-color: #337ab7;'>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Flight</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Source</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Destination</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Info</th>";
                ticket += "</tr>";
                foreach (var fd in result.Object.flightSegments)
                {
                    ticket += "<tr><td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.Logo.Split('.')[0] + "<br/>" + fd.AirlineCode + "-" + fd.FlightNumber + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.Origin + "<br/>" + fd.DepartureDateTime.ToLongDateString() + " " + fd.DepartureDateTime.ToShortTimeString() + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.Destination + "<br/>" + fd.ArrivalDatetime.ToLongDateString() + " " + fd.ArrivalDatetime.ToShortTimeString() + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.ClassCodeDesc + "<br/>" + result.Object.flight.FareType + "</td></tr>";
                }
                ticket += "</tbody></table></div>";

                ticket += "<p style='margin-top: 1em;margin-bottom: -.5em;font-weight: 700;font-size: 12px;'>Traveller(s) Information</p>";
                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding:0px;'>";

                ticket += "<table style='border-collapse: collapse;width: 100%;max-width: 100%;margin-bottom: 1rem;background-color: transparent;'>";
                ticket += "<tbody style='display: table-row-group;vertical-align: middle;border-color: inherit;'>";
                ticket += "<tr style='background-color: #337ab7;'>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>S.No.</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Passenger Name</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Type</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>E-Ticket No</th>";
                ticket += "</tr>";
                var i = 0;
                foreach (var fd in result.Object.flightDetails)
                {
                    ticket += "<tr><td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + (++i) + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.Title + " " + fd.FirstName + " " + fd.LastName + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.Type + "</td>";
                    ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>" + fd.TicketNumber + "</td></tr>";
                }
                ticket += "</tbody></table></div>";

                ticket += "<p style='margin-top: 1em;margin-bottom: -.5em;font-weight: 700;font-size:12px;'>Fare Information</p>";
                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding:0px;'>";

                ticket += "<table style='border-collapse: collapse;width: 100%;max-width: 100%;margin-bottom: 1rem;background-color: transparent;'>";
                ticket += "<tbody style='display: table-row-group;vertical-align: middle;border-color: inherit;'>";
                ticket += "<tr style='background-color: #337ab7;'>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Basic Amt</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'> Taxes, Surcharges & Fees</th>";
                ticket += "<th style='vertical-align: top;padding: .75rem;border: none;color: #fff;text-align: center;'>Total</th>";
                ticket += "</tr>";
                ticket += "<tr><td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>₹ " + result.Object.flight.BasicAmount + "</td>";
                ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>₹ " + (result.Object.flight.GrossAmount - result.Object.flight.BasicAmount) + "</td>";
                ticket += "<td style='vertical-align: top;padding: .75rem;border: none;text-align: center;'>₹ " + result.Object.flight.GrossAmount + "</td></tr>";
                ticket += "</tbody></table></div>";

                ticket += "<p style='margin-top: 1em;margin-bottom: -.5em;font-weight: 700;font-size: 12px;'>Rules & Regulations</p>";
                ticket += "<div style='-webkit-box-flex: 1;flex: 1 1 auto;padding:0px;>";
                ticket += "<ol  type='1'>";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'> For Any queries feel free to contact us at our helpline numbers +91-9538012000.</li>";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'> Passenger is requested to check-in 2hrs prior to scheduled departure.</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'> All Passengers including children and infants must present valid identity proof at check-in. It is your responsibility";
                ticket += " to ensure you have the appropriate travel documents at all times.</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>Changes / Cancellations to booking must be made at least 6 hours prior to scheduled departure time or else";
                ticket += " should be cancelled directly from the respective airlinesticket cancellation the transaction fee will";
                ticket += " not be refunded.We are not responsible for any losses if the request is received less than 6 hours before";
                ticket += " departure.";
                ticket += "</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>If any flight is cancelled or rescheduled by the airline authority, passenger is requested to get a stamped/ ";
                ticket += " endorsed copy of the ticket to process the refund.</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>The No Show refund should be collected within 90 days from departure date.</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>We charge a service fee of Rs. 100.0 per ticket/ passenger for all cancellations, apart from airline charges.</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>We are not responsible for any Flight delay / Cancellation from airline's end.</li>";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>Kindly contact the airline at least 24 hrs before to reconfirm your flight detail giving reference of Airline";
                ticket += " PNR Number.";
                ticket += "</li> ";
                ticket += "<li style='color: #000;font-size: 11px;font-family: Lato,sans-serif;'>You agree to abide by terms and conditions laid forth by Travelothon Private Limited.</li> ";
                ticket += "</ol></div>";
                ticket += "</div></div></div>";

                var emailService = new EMail();
                var Destination = result.Object.flight.Email;
                await emailService.SendAsync(new IdentityMessage()
                {
                    Body = Body + ticket,

                    Subject = airline + " flight booking confirmed with PNR: " + result.Object.flight.PNR + " for " + result.Object.flight.DOJ.ToString("dd-MMM-yyyy"),
                    Destination = Destination
                },
                  CC: "",
                  BCC: "",
                  hideFooter: true
                 );
            }
            catch (Exception e)
            {
                var emailService = new EMail();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = "Travelothon Flight Booking Error.Environment " + Request.RequestUri.Host.ToString(),
                    Destination = Destination
                });
            }
            #endregion
        }

        private async Task AgentLowBalanceHelper()
        {
            var agentModel = new AgentCreditBalanceRequest
            {
                Authentication = new Domestic_Airlines.Authentication
                {
                    LoginId = auth.LoginId,
                    Password = auth.Password
                }
            };
            var travelothonDomestic = new ApiTravelothon();
            var objectDto = JsonConvert.DeserializeObject<AgentCreditBalanceResponse>(await travelothonDomestic.TravelothonService(
                JsonConvert.SerializeObject(agentModel), "DomesticFlight", "GetAgentCreditBalance", "POST"));

            double agentBalance = 0;
            if (objectDto.ResponseStatus == 1)
                agentBalance = objectDto.AgentCreditBalanceOutput.RemainingAmount;

            if (agentBalance < 10000)
            {
                var emailService = new EMail();
                var Subject = "Hermes Low Balance Notification.Environment " + Request.RequestUri.Host.ToString();
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");
                emailService.SendAsync(new IdentityMessage()
                {
                    Body = "Available Balance: " + agentBalance,
                    Subject = Subject,
                    Destination = Destination
                });
            }
        }

        private async Task<double> TraxBalanceCheck()
        {
            var agentModel = new AgentCreditBalanceRequest
            {
                Authentication = new Domestic_Airlines.Authentication
                {
                    LoginId = auth.LoginId,
                    Password = auth.Password
                }
            };
            var travelothonDomestic = new ApiTravelothon();
            var objectDto = JsonConvert.DeserializeObject<AgentCreditBalanceResponse>(await travelothonDomestic.TravelothonService(
                JsonConvert.SerializeObject(agentModel), "DomesticFlight", "GetAgentCreditBalance", "POST"));

            double agentBalance = 0;
            if (objectDto.ResponseStatus == 1)
                agentBalance = objectDto.AgentCreditBalanceOutput.RemainingAmount;
            return agentBalance;
        }
        private async Task<decimal> GetBalance()
        {
            var GUid = User.Identity.GetUserId();
            using (var con = new ApplicationDbContext())
            {
                var userObj = await con.PUserProfile.FindAsync(GUid);
                return userObj.Balance;
            }
        }
        #endregion  
        protected override void Dispose(bool disposing)
        {
            if (disposing && context != null)
            {
                context.Dispose();
                context = null;
            }

            base.Dispose(disposing);
        }
    }

}
