using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportPeopleSites.Models;
using USDA_ARS.ImportPeopleSites.Objects;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportPeopleSites
{
    class Program
    {
        static string LOG_FILE_TEXT = "";

        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        static List<ModeCodeLookup> MODE_CODE_LIST = null;
        static List<PeopleFolderLookup> PEOPLE_FOLDER_LIST = null;

        static void Main(string[] args)
        {
            bool forceCacheUpdate = false;

            if (args != null && args.Length >= 1)
            {
                if (args[0] == "force-cache-update")
                {
                    forceCacheUpdate = true;
                }
            }

            Logs.AddLog(ref LOG_FILE_TEXT, "Getting Mode Codes From Umbraco...");
            ModeCodes.GenerateModeCodeList(ref MODE_CODE_LIST, ref LOG_FILE_TEXT, forceCacheUpdate);
            Logs.AddLog(ref LOG_FILE_TEXT, "Done. Count: " + MODE_CODE_LIST.Count);

            Logs.AddLog(ref LOG_FILE_TEXT, "");

            Logs.AddLog(ref LOG_FILE_TEXT, "Getting People Folders From Umbraco...");
            PeopleFolders.GenerateModeCodeFolderList(ref PEOPLE_FOLDER_LIST, ref LOG_FILE_TEXT, MODE_CODE_LIST, forceCacheUpdate);
            Logs.AddLog(ref LOG_FILE_TEXT, "Done. Count: " + PEOPLE_FOLDER_LIST.Count);

            Logs.AddLog(ref LOG_FILE_TEXT, "");


            if (PEOPLE_FOLDER_LIST != null && PEOPLE_FOLDER_LIST.Any())
            {


                // LOOP THROUGH VALID MODE CODES
                {
                    string modeCode = ""; // Get the mode code in the xx-xx-xx-xx format

                    PeopleFolderLookup peopleFolder = PEOPLE_FOLDER_LIST.Where(p => p.ModeCode == modeCode).FirstOrDefault();

                    if (peopleFolder != null)
                    {
                        Logs.AddLog(ref LOG_FILE_TEXT, "Mode Code: " + modeCode);

                        int peopleFolderUmbracoId = peopleFolder.PeopleFolderUmbracoId;




                        // ADD PEOPLE SITES HERE: (LOOP THROUGH VALID/ACTIVE PEOPLE IN THE MODE CODE)
                        {
                            int personId = 0; // GET PERSON ID
                            string personName = ""; //GET PERSON NAME
                            string personSiteHtml = ""; // GET PERSON SITE HTML

                            personSiteHtml = CleanHtml.CleanUpHtml(personSiteHtml);

                            // Make sure the HTML is not empty
                            if (false == string.IsNullOrWhiteSpace(personSiteHtml))
                            {
                                ApiResponse apiResponse = AddUmbracoPersonPage(peopleFolderUmbracoId, personId, personName, personSiteHtml);

                                if (apiResponse != null && apiResponse.Success)
                                {
                                    Logs.AddLog(ref LOG_FILE_TEXT, " - Added Person ("+ personId + "): " + personName);
                                }
                                else
                                {
                                    Logs.AddLog(ref LOG_FILE_TEXT, " - !ERROR! Person not added (" + personId + "): " + personName + " | " + apiResponse.Message);
                                }
                            }
                        }
                    }
                }
            }
        }


        static ApiResponse AddUmbracoPersonPage(int parentId, int personId, string name, string body)
        {
            ApiContent content = new ApiContent();

            content.Id = 0;
            content.Name = name;
            content.ParentId = parentId;
            content.DocType = "PersonSite";
            content.Template = "PersonSite"; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("bodyText", body)); // HTML of person site
            properties.Add(new ApiProperty("personLink", personId)); // Person's ID                                                                                         
            properties.Add(new ApiProperty("oldUrl", "/pandp/people/people.htm?personid=" + personId)); // current URL               

            content.Properties = properties;

            content.Save = 2;

            ApiRequest request = new ApiRequest();

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);
            request.ApiKey = API_KEY;

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            return responseBack;
        }
    }
}
