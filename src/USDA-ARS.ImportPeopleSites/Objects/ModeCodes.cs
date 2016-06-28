using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportPeopleSites.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportPeopleSites.Objects
{
    public class ModeCodes
    {
        public static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        public static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        public static void GenerateModeCodeList(ref List<ModeCodeLookup> modeCodeList, ref string logFileText, bool forceCacheUpdate)
        {
            modeCodeList = GetModeCodeLookupCache();

            if (true == forceCacheUpdate || modeCodeList == null || modeCodeList.Count <= 0)
            {
                modeCodeList = CreateModeCodeLookupCache(ref modeCodeList, ref logFileText);
            }
        }


        public static List<ModeCodeLookup> GetModeCodeLookupCache()
        {
            string filename = "mode-code-cache.txt";
            List<ModeCodeLookup> modeCodeList = new List<ModeCodeLookup>();

            if (true == File.Exists(filename))
            {
                using (StreamReader sr = File.OpenText(filename))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] lineArray = s.Split('|');

                        modeCodeList.Add(new ModeCodeLookup() { ModeCode = lineArray[0], UmbracoId = Convert.ToInt32(lineArray[1]), Url = lineArray[2] });
                    }
                }
            }

            return modeCodeList;
        }


        public static List<ModeCodeLookup> CreateModeCodeLookupCache(ref List<ModeCodeLookup> modeCodeList, ref string logFileText)
        {
            List<ModeCodeLookup> modeCodeLookupList = new List<ModeCodeLookup>();

            modeCodeList = GetModeCodesAll(ref modeCodeList, ref logFileText);

            StringBuilder sb = new StringBuilder();

            if (modeCodeList != null)
            {
                foreach (ModeCodeLookup modeCodeItem in modeCodeList)
                {
                    sb.AppendLine(modeCodeItem.ModeCode + "|" + modeCodeItem.UmbracoId + "|" + modeCodeItem.Url);
                }

                using (FileStream fs = File.Create("mode-code-cache.txt"))
                {
                    // Add some text to file
                    Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(fileText, 0, fileText.Length);
                }
            }

            return modeCodeList;
        }


        public static List<ModeCodeLookup> GetModeCodesAll(ref List<ModeCodeLookup> modeCodeList, ref string logFileText)
        {
            List<ModeCodeLookup> modeCodeLookupList = new List<ModeCodeLookup>();
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

                                modeCodeList.Add(new ModeCodeLookup { ModeCode = modeCode.Value.ToString(), UmbracoId = node.Id, Url = node.Url });

                                Logs.AddLog(ref logFileText, " - Adding ModeCode (" + modeCode.Value + "):" + node.Name);
                            }
                        }
                    }
                }
            }

            return modeCodeList;
        }
    }
}
