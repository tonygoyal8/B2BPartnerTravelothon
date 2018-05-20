using B2BPartnerTravelothon.Constants;
using B2BPartnerTravelothon.Models;
using B2BPartnerTravelothon.Models.Payment;
using B2BPartnerTravelothon.ViewModel.PaymentGateway;
using B2BPartnerTravelothon.ViewModel.Shared;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace B2BPartnerTravelothon.Controllers
{
    public class PaymentGatewayController : ApiController
    {
        public PaymentGatewayController()
        {

        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateRequest(PaymentRequest payment)
        {
            //string json = JsonConvert.SerializeObject(payment);
            string json = "{\"buyer_name\":\"" + payment.buyer_name + "\",\"email\":\"" + payment.email + "\",\"phone\":\"" + payment.phone + "\",\"amount\":\"" + payment.amount + "\",\"purpose\":\"" + payment.purpose + "\",\"redirect_url\":\"" + payment.redirect_url + "\",\"webhook\":\"" + payment.webhook + "\"}";
            var result = await GetURL(json, "payment-requests", RequestType.POST.ToString());
            if (result.valid)
            {
                using (var con = new ApplicationDbContext())
                {
                    var pay = new PaymentDto(result.Object.payment_request, (int)StatusFlag.Payment_Initiated);
                    con.Payments.Add(pay);
                    await con.SaveChangesAsync();
                }
            }
            return Ok(result);
        }

        [ResponseType(typeof(ObjectDto<PaymentStatusCheckResponse>))]
        [HttpPost]
        public async Task<IHttpActionResult> GetPaymentStatus(PaymentStatusCheck paymentChk)
        {
            int count = 0;
            int status = (int)StatusFlag.Payment_Inprogress;
            ObjectDto<PaymentStatusCheckResponse> result = new ObjectDto<PaymentStatusCheckResponse>();
            var Message = new Messages();
            Message.Type = "INFO";
            Message.Message = "Your payment request is in-progress";
            int id = 0;
            using (var con = new ApplicationDbContext())
            {
                var paymentExist = con.Payments.FirstOrDefault(x => x.PaymentRequestId == paymentChk.PaymentRequestId && x.Status == (int)StatusFlag.Payment_Initiated);
                if (paymentExist != null)
                {
                    id = paymentExist.Id;
                    pay:
                    var data = await GetURL("", "payments/" + paymentChk.PaymentId, RequestType.GET.ToString());
                    if (data.valid)
                    {
                        if (data.Object.payment.status != PaymentGatewayStatus.Credit.ToString() && data.Object.payment.status != PaymentGatewayStatus.Failed.ToString() && count < 3)
                        {
                            count++;
                            goto pay;
                        }
                        if (data.Object.payment.status == PaymentGatewayStatus.Credit.ToString())
                        {
                            status = (int)StatusFlag.Payment_Success;
                            Message.Type = "SUCCESS";
                            Message.Message = "Your payment request is successfully completed";
                           
                        }
                        if (data.Object.payment.status == PaymentGatewayStatus.Failed.ToString())
                        {
                            status = (int)StatusFlag.Payment_Failed;
                            Message.Type = "ERROR";
                            Message.Message = "Your payment request has failed";
                        }

                    }
                    paymentExist.LastModifiedDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    paymentExist.PaymentId = paymentChk.PaymentId;
                    paymentExist.Status = status;
                    paymentExist.Fees = data.Object.payment.fees;
                    con.Entry(paymentExist).State = EntityState.Modified;

                   
                    await con.SaveChangesAsync();

                    if (status == (int)StatusFlag.Payment_Success)
                    {
                        result.valid = true;
                        var paymentStatusCheckResponse = new PaymentStatusCheckResponse();
                        paymentStatusCheckResponse.TransactionId = id;
                        paymentStatusCheckResponse.ServiceType = (int)ServiceType.Flight_Domestic;
                        result.Object = paymentStatusCheckResponse;
                    }
                }
            }
            result.messages.Add(Message);
            return Ok(result);
        }
        private async Task<ObjectDto<dynamic>> GetURL(string json, string methodName, string requestType)
        {
            ObjectDto<dynamic> result = new ObjectDto<dynamic>();
            try
            {
                var payUrl = System.Configuration.ConfigurationManager.AppSettings.Get("PaymentUrl");
                var key = System.Configuration.ConfigurationManager.AppSettings.Get("X-Api-Key");
                var token = System.Configuration.ConfigurationManager.AppSettings.Get("X-Auth-Token");

                string url = payUrl + methodName + "/";
                Uri uri = new Uri(url);
                WebRequest webRequest = WebRequest.Create(uri);
                webRequest.Headers.Add("X-Api-Key", key);
                webRequest.Headers.Add("X-Auth-Token", token);
                webRequest.ContentType = "application/json";
                webRequest.Method = requestType;
                if (!String.IsNullOrEmpty(json))
                {
                    byte[] postBytes = Encoding.UTF8.GetBytes(json);
                    Stream requestStream = await webRequest.GetRequestStreamAsync();
                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();
                }

                using (WebResponse response = await webRequest.GetResponseAsync())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        var soapResult = rd.ReadToEnd();
                        dynamic data = JsonConvert.DeserializeObject(soapResult);
                        if (data["success"] == false)
                        {
                            var messages = new Messages();
                            result.messages.Add(messages);
                        }
                        else
                        {
                            result.valid = true;
                            result.Object = data;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var messages = new Messages();
                messages.Message = e.Message;
                result.messages.Add(messages);
            }
            return result;
        }

    }
}

