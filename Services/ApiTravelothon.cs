using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;


namespace B2BPartnerTravelothon.Services
{
    public class ApiTravelothon
    {
        public async Task<string> TravelothonService(string json,string controllerName, string methodName, string requestType)
        {

            var travelothonUrl =System.Configuration.ConfigurationManager.AppSettings.Get("travelothonUrl");

             string url = travelothonUrl+"/"+ controllerName + "/" + methodName ;
            Uri uri = new Uri(url);
            WebRequest webRequest = WebRequest.Create(uri);

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
                    return soapResult;

                }
            }
        }
    }
}