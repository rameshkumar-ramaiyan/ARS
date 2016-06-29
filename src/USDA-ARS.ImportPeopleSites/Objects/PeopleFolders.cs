using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportPeopleSites.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportPeopleSites.Objects
{
    public class PeopleFolders
    {
        public static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        public static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        public static void GenerateModeCodeFolderList(ref List<PeopleFolderLookup> peopleFolderList, ref string logFileText, List<ModeCodeLookup> modeCodeList, bool forceCacheUpdate)
        {
            peopleFolderList = GetPeopleFolderLookupCache();

            if (true == forceCacheUpdate || peopleFolderList == null || peopleFolderList.Count <= 0)
            {
                peopleFolderList = CreatePeopleFolderLookupCache(ref peopleFolderList, ref logFileText, modeCodeList);
            }
        }

        public static List<PeopleFolderLookup> GetPeopleFolderLookupCache()
        {
            string filename = "people-folder-cache.txt";
            List<PeopleFolderLookup> modeCodeList = new List<PeopleFolderLookup>();

            if (true == File.Exists(filename))
            {
                using (StreamReader sr = File.OpenText(filename))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] lineArray = s.Split('|');

                        modeCodeList.Add(new PeopleFolderLookup() { ModeCode = lineArray[0], PeopleFolderUmbracoId = Convert.ToInt32(lineArray[1]) });
                    }
                }
            }

            return modeCodeList;
        }

        public static List<PeopleFolderLookup> CreatePeopleFolderLookupCache(ref List<PeopleFolderLookup> peopleFolderList, ref string logFileText, List<ModeCodeLookup> modeCodeList)
        {
            List<PeopleFolderLookup> modeCodeFolderList = new List<PeopleFolderLookup>();

            modeCodeFolderList = GetPeopleFoldersAll(ref peopleFolderList, ref logFileText, modeCodeList);

            StringBuilder sb = new StringBuilder();

            if (modeCodeFolderList != null)
            {
                foreach (PeopleFolderLookup modeCodeItem in modeCodeFolderList)
                {
                    sb.AppendLine(modeCodeItem.ModeCode + "|" + modeCodeItem.PeopleFolderUmbracoId);
                }

                using (FileStream fs = File.Create("people-folder-cache.txt"))
                {
                    // Add some text to file
                    Byte[] fileText = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(fileText, 0, fileText.Length);
                }
            }

            return modeCodeFolderList;
        }

        public static List<PeopleFolderLookup> GetPeopleFoldersAll(ref List<PeopleFolderLookup> peopleFolderList, ref string logFileText, List<ModeCodeLookup> modeCodeList)
        {
            List<PeopleFolderLookup> modeCodeFolderList = new List<PeopleFolderLookup>();

            if (modeCodeList != null && modeCodeList.Any())
            {
                foreach (ModeCodeLookup modeCodeFound in modeCodeList)
                {
                    ApiRequest request = new ApiRequest();

                    request.ApiKey = API_KEY;

                    request.ContentList = new List<ApiContent>();

                    request.ContentList.Add(new ApiContent() { Id = modeCodeFound.UmbracoId });

                    ApiResponse responseBack = ApiCalls.PostData(request, "Get");

                    if (responseBack != null && responseBack.Success)
                    {
                        if (responseBack.ContentList != null && responseBack.ContentList.Any() && responseBack.ContentList[0].ChildContentList != null && responseBack.ContentList[0].ChildContentList.Any())
                        {
                            ApiContent nodeFolder = responseBack.ContentList[0].ChildContentList.Where(p => p.DocType == "PeopleFolder").FirstOrDefault();

                            if (nodeFolder != null)

                                foreach (ApiContent node in responseBack.ContentList)
                                {
                                    if (node != null)
                                    {
                                        //string 

                                        modeCodeFolderList.Add(new PeopleFolderLookup { ModeCode = modeCodeFound.ModeCode, PeopleFolderUmbracoId = node.Id });
                                        Logs.AddLog(ref logFileText, " - Adding People Folder: " + modeCodeFound.ModeCode + " (" + node.Id + ")");
                                    }
                                }
                        }
                    }
                }
            }






            return modeCodeFolderList;
        }
    }
}
