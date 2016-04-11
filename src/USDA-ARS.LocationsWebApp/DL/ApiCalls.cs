using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class ApiCalls
    {
        protected static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        /// <summary>
        /// Posts a JSON Content Object to the Umbraco API
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static ApiResponse PostData(ApiRequest request, string endPoint = "Post")
        {
            ApiResponse response = null;
            string apiUrl = API_URL;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl + endPoint));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            http.Timeout = 3600000;
            string parsedContent = JsonConvert.SerializeObject(request);
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var httpApiResponse = http.GetResponse();

            var stream = httpApiResponse.GetResponseStream();
            var sr = new StreamReader(stream);
            var httpApiResponseStr = sr.ReadToEnd();

            response = JsonConvert.DeserializeObject<ApiResponse>(httpApiResponseStr);

            return response;
        }
    }
}