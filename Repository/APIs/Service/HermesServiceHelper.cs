using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace B2BPartnerTravelothon.Repository.APIs.Service
{
    public class HermesServiceHelper
    {
        static HttpClient client;
        //public static HttpWebRequest CreateWebRequest()
        //{
        //    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://115.248.39.80/HermesMobAPI/HermesMobile.svc/jsonser vice/GetRechargeOperators");
        //    webRequest.ContentType = "application/json";
        //    webRequest.Method = "POST";
        //    return webRequest;
        //}
        public HermesServiceHelper()
        {
            client = new HttpClient();
        }
        public static async Task<object> MakeHermesRequest(string requestUrl, object JSONRequest, string JSONmethod)
        {

            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                //WebRequest WR = WebRequest.Create(requestUrl);   
                string sb = JsonConvert.SerializeObject(JSONRequest);
                request.Method = JSONmethod;
                // "POST";
                request.ContentType = "application/json";   
                Byte[] bt = Encoding.UTF8.GetBytes(sb);
                Stream st =await request.GetRequestStreamAsync();
                st.Write(bt, 0, bt.Length);
                st.Close();


                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {

                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).", response.StatusCode,
                    response.StatusDescription));

                    // DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                    // object objResponse = JsonConvert.DeserializeObject();
                    Stream stream1 = response.GetResponseStream();   
                    StreamReader sr = new StreamReader(stream1);
                    string strsb = sr.ReadToEnd();
                    object objResponse = JsonConvert.DeserializeObject(strsb);

                    return objResponse;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return null;
            }
        }

    }
}