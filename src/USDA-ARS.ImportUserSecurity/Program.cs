using Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportUserSecurity.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportUserSecurity
{
   class Program
   {
      public static string UmbracoUserConnectionString = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;
      public static string ExcelFilePath = ConfigurationManager.AppSettings.Get("UserExcel:Path");

      static string LOG_FILE_TEXT = "";

      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
      static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

      static DateTime TIME_STARTED = DateTime.MinValue;
      static DateTime TIME_ENDED = DateTime.MinValue;

      static List<UmbracoPageLookup> UMBRACO_PAGE_LIST = null;

      static void Main(string[] args)
      {
         TIME_STARTED = DateTime.Now;

         AddLog("-= UPDATING UMBRACO USER START NODES =-");
         AddLog("");

         AddLog("Getting Mode Codes From Umbraco...");
         GenerateModeCodeList(false);
         AddLog("Done. Count: " + UMBRACO_PAGE_LIST.Count);

         AddLog("Adding Special Pages...");
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "ARS Home", UmbracoId = 1075 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "National Programs", UmbracoId = 31699 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "Office of International Research Programs", UmbracoId = 130729 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "Office of Outreach, Diversity, and Equal Opportunity", UmbracoId = 130739 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "Office of Pest Management Policy", UmbracoId = 130739 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "Office of Scientific Quality Review", UmbracoId = 2133 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "Office of Technology Transfer", UmbracoId = 2220 });
         UMBRACO_PAGE_LIST.Add(new UmbracoPageLookup() { ModeCodeOrPage = "National Advisory Council for Office Professionals", UmbracoId = 130737 });
         AddLog("Done. Count: " + UMBRACO_PAGE_LIST.Count);

         AddLog("");
         AddLog("Gathering data from Excel file...");

         FileStream stream = File.Open(ExcelFilePath, FileMode.Open, FileAccess.Read);
         IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

         excelReader.IsFirstRowAsColumnNames = true;

         while (excelReader.Read())
         {
            string username = "";
            string modeCodeOrPage = "";

            username = excelReader.GetString(0);
            modeCodeOrPage = excelReader.GetString(2);

            AddLog("Looking up user in Umbraco...");

            int userId = GetUserId(username);

            if (userId > 0)
            {
               string modeCodeTest = modeCodeOrPage;

               if (modeCodeTest.Length == 10)
               {
                  modeCodeOrPage = USDA_ARS.Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCodeOrPage);
                  modeCodeOrPage = USDA_ARS.Umbraco.Extensions.Helpers.ModeCodes.ModeCodeAddDashes(modeCodeOrPage);
               }

               UmbracoPageLookup pageLookup = UMBRACO_PAGE_LIST.Where(p => p.ModeCodeOrPage.ToLower() == modeCodeOrPage.ToLower()).FirstOrDefault();

               if (pageLookup != null && false == string.IsNullOrWhiteSpace(modeCodeOrPage))
               {
                  UpdateUserStartNode(userId, pageLookup.UmbracoId, username);
                  AddLog("Start node udpated [" + username + "]: " + modeCodeOrPage);
               }
               else
               {
                  AddLog("Unable to find modeCode/page: " + modeCodeOrPage);
               }
            }

            //excelReader.GetInt32(0);
         }




         AddLog("");

         AddLog("");
         AddLog("");
         AddLog("");
         AddLog("/// IMPORT COMPLETE ///");
         AddLog("");

         TIME_ENDED = DateTime.Now;

         TimeSpan timeLength = TIME_ENDED.Subtract(TIME_STARTED);

         AddLog("/// Time to complete: " + timeLength.ToString(@"hh") + " hours : " + timeLength.ToString(@"mm") + " minutes : " + timeLength.ToString(@"ss") + " seconds ///");

         Console.ReadKey();
      }



      static void GenerateModeCodeList(bool forceCacheUpdate)
      {
         UMBRACO_PAGE_LIST = GetModeCodeLookupCache();

         if (true == forceCacheUpdate || UMBRACO_PAGE_LIST == null || UMBRACO_PAGE_LIST.Count <= 0)
         {
            UMBRACO_PAGE_LIST = CreateModeCodeLookupCache();
         }
      }

      static List<UmbracoPageLookup> GetModeCodeLookupCache()
      {
         string filename = "umbraco-lookup-cache.txt";
         List<UmbracoPageLookup> modeCodeList = new List<UmbracoPageLookup>();

         if (true == File.Exists(filename))
         {
            using (StreamReader sr = File.OpenText(filename))
            {
               string s = "";
               while ((s = sr.ReadLine()) != null)
               {
                  string[] lineArray = s.Split('~');

                  modeCodeList.Add(new UmbracoPageLookup() { ModeCodeOrPage = lineArray[0], UmbracoId = Convert.ToInt32(lineArray[1]) });
               }
            }
         }

         return modeCodeList;
      }

      static List<UmbracoPageLookup> CreateModeCodeLookupCache()
      {
         List<UmbracoPageLookup> modeCodeList = new List<UmbracoPageLookup>();

         modeCodeList = GetModeCodesAll();

         StringBuilder sb = new StringBuilder();

         if (modeCodeList != null)
         {
            foreach (UmbracoPageLookup modeCodeItem in modeCodeList)
            {
               sb.AppendLine(modeCodeItem.ModeCodeOrPage + "~" + modeCodeItem.UmbracoId);
            }

            using (FileStream fs = File.Create("umbraco-lookup-cache.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
               fs.Write(fileText, 0, fileText.Length);
            }
         }

         return modeCodeList;
      }

      static List<UmbracoPageLookup> GetModeCodesAll()
      {
         List<UmbracoPageLookup> modeCodeLookupList = new List<UmbracoPageLookup>();
         ApiRequest request = new ApiRequest();

         request.ApiKey = API_KEY;

         ApiResponse responseBack = ApiCalls.PostData(request, "GetAllModeCodeNodes");

         if (responseBack != null && responseBack.Success)
         {
            if (responseBack.ContentList != null && responseBack.ContentList.Any())
            {
               foreach (ApiContent node in responseBack.ContentList)
               {
                  if (node != null)
                  {
                     ApiProperty modeCode = node.Properties.Where(p => p.Key == "modeCode").FirstOrDefault();

                     if (modeCode != null)
                     {
                        string oldUrl = "";

                        ApiProperty oldUrlProp = node.Properties.Where(p => p.Key == "oldUrl").FirstOrDefault();

                        if (oldUrlProp != null)
                        {
                           oldUrl = oldUrlProp.Value.ToString();
                        }

                        modeCodeLookupList.Add(new UmbracoPageLookup { ModeCodeOrPage = modeCode.Value.ToString(), UmbracoId = node.Id });

                        AddLog(" - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
                     }
                  }
               }
            }
         }

         return modeCodeLookupList;
      }





      static int GetUserId(string username)
      {
         int userId = 0;
         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT id FROM [umbracoUser] WHERE userName = @username";

         List<int> docList = db.Query<int>(sql, new { username = username }).ToList();

         if (docList != null && docList.Any())
         {
            userId = docList.FirstOrDefault();
         }

         return userId;
      }


      static int UpdateUserStartNode(int userId, int startNodeId, string userName)
      {
         var db = new Database("umbracoDbDSN");

         UmbracoUser umbracoUser = new UmbracoUser();

         umbracoUser.id = userId;
         umbracoUser.startStructureID = startNodeId;
         umbracoUser.userName = userName;

         db.Update("umbracoUser", "id", umbracoUser);

         return 1;
      }


      static void AddLog(string line)
      {
         Debug.WriteLine(line);
         Console.WriteLine(line);
         LOG_FILE_TEXT += line + "\r\n";
      }
   }
}
