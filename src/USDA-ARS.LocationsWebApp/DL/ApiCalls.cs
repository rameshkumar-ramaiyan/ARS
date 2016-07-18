using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
   public class ApiCalls
   {
      public static string LOG_FILE_TEXT = "";

      protected static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

      /// <summary>
      /// Posts a JSON Content Object to the Umbraco API
      /// </summary>
      /// <param name="content"></param>
      /// <returns></returns>
      public static ApiResponse PostData(ApiRequest request, string endPoint = "Post")
      {
         ApiResponse response = null;
         bool errorDetected = false;

         try
         {
            response = PostDataProcess(request, endPoint);
         }
         catch (Exception ex)
         {
            AddLog("///////////////////////////////");
            AddLog("// ERROR DETECTED ON UMBRACO API");
            AddLog("///////////////////////////////");
            AddLog("// ERROR:");
            AddLog(ex.ToString());
            AddLog("///////////////////////////////");
            AddLog("// POST DATA:");
            AddLog(JsonConvert.SerializeObject(request));
            AddLog("///////////////////////////////");
            AddLog("");
            AddLog("");

            errorDetected = true;
         }


         // TRY 2
         if (true == errorDetected)
         {
            errorDetected = false;
            AddLog("// WAITING 10 SECONDS THEN TRYING AGAIN");
            AddLog("///////////////////////////////");
            Wait(10);

            try
            {
               response = PostDataProcess(request, endPoint);
            }
            catch (Exception ex)
            {
               AddLog("///////////////////////////////");
               AddLog("// ATTEMPT 2: ERROR DETECTED ON UMBRACO API");
               AddLog("///////////////////////////////");
               AddLog("// ERROR:");
               AddLog(ex.ToString());
               AddLog("///////////////////////////////");
               AddLog("// POST DATA:");
               AddLog(JsonConvert.SerializeObject(request));
               AddLog("///////////////////////////////");
               AddLog("");
               AddLog("");

               errorDetected = true;
            }
         }

         // TRY 3
         if (true == errorDetected)
         {
            errorDetected = false;
            AddLog("// WAITING 30 SECONDS THEN TRYING AGAIN");
            AddLog("///////////////////////////////");
            Wait(30);

            try
            {
               response = PostDataProcess(request, endPoint);
            }
            catch (Exception ex)
            {
               AddLog("///////////////////////////////");
               AddLog("// ATTEMPT 3: ERROR DETECTED ON UMBRACO API");
               AddLog("///////////////////////////////");
               AddLog("// ERROR:");
               AddLog(ex.ToString());
               AddLog("///////////////////////////////");
               AddLog("// POST DATA:");
               AddLog(JsonConvert.SerializeObject(request));
               AddLog("///////////////////////////////");
               AddLog("");
               AddLog("");

               errorDetected = true;
            }
         }

         // TRY 4
         if (true == errorDetected)
         {
            errorDetected = false;
            AddLog("// WAITING 2 MINUTES THEN TRYING FINAL ATTEMPT");
            AddLog("///////////////////////////////");
            Wait(120);

            try
            {
               response = PostDataProcess(request, endPoint);
            }
            catch (Exception ex)
            {

               AddLog("///////////////////////////////");
               AddLog("// ATTEMPT 4: ERROR DETECTED ON UMBRACO API");
               AddLog("///////////////////////////////");
               AddLog("// ERROR:");
               AddLog(ex.ToString());
               AddLog("///////////////////////////////");
               AddLog("// POST DATA:");
               AddLog(JsonConvert.SerializeObject(request));
               AddLog("///////////////////////////////");
               AddLog("");
               AddLog("");
               errorDetected = true;

               using (FileStream fs = File.Create("ERROR_LOG_FILE.txt"))
               {
                  // Add some text to file
                  Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
                  fs.Write(fileText, 0, fileText.Length);
               }
            }
         }

         return response;
      }


      public static ApiResponse PostDataProcess(ApiRequest request, string endPoint = "Post")
      {
         ApiResponse response = null;
         string apiUrl = API_URL;

         var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl + endPoint));
         http.Accept = "application/json";
         http.ContentType = "application/json";
         http.Method = "POST";
         http.Timeout = 10800000;
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

      static void AddLog(string line)
      {
         Debug.WriteLine(line);
         Console.WriteLine(line);
         LOG_FILE_TEXT += line + "\r\n";
      }

      static void Wait(int seconds)
      {
         int timeSecs = 0;

         while (timeSecs <= seconds)
         {
            Console.Write(".");
            Thread.Sleep(1000);

            timeSecs++;
         }
         Console.WriteLine();
         Console.WriteLine("Trying again...");
      }
   }
}