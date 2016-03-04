using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class PersonSite
    {
        protected static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        /// <summary>
        /// Adds people sites for a particluar region/area or research unit
        /// </summary>
        /// <param name="modeCode"></param>
        /// <returns></returns>
        public static ApiResponse AddPeopleSites(string modeCode)
        {
            ApiResponse responsePage = new ApiResponse();

            // Get the umbraco page by the mode code (Region/Area or Research Unit)
            responsePage = GetCalls.GetNodeByModeCode(modeCode);

            if (responsePage != null && true == responsePage.Success && responsePage.ContentList != null && responsePage.ContentList.Count > 0)
            {
                ApiContent peopleFolder = responsePage.ContentList[0].ChildContentList.Where(p => p.DocType == "PeopleFolder").FirstOrDefault();

                if (peopleFolder != null)
                {
                    List<ApiContent> contentPeopleSites = new List<ApiContent>();

                    // ADD PEOPLE SITES HERE: (LOOP)
                    {
                        int personId = 42111;
                        string personName = "Colin S. Brent";
                        string personSiteHtml = "<p>Hello!</p>";

                        ApiContent personSite = GeneratePersonSiteContent(peopleFolder.Id, personId, personName, personSiteHtml);

                        if (personSite != null)
                        {
                            contentPeopleSites.Add(personSite);
                        }
                    } // END LOOP

                    ApiRequest request = new ApiRequest();

                    request.ContentList = contentPeopleSites;
                    request.ApiKey = API_KEY;

                    ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                    return responseBack;
                }
                else
                {
                    throw new Exception("Unable to find Umbraco People Folder within Site: " + responsePage.ContentList[0].Name);
                }

            }
            else
            {
                throw new Exception("Unable to find Umbraco page by Mode Code: " + modeCode);
            }
        }


        /// <summary>
        /// Generates a ApiContent object of person site for API
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="personId"></param>
        /// <param name="name"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static ApiContent GeneratePersonSiteContent(int parentId, int personId, string name, string body)
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

            return content;
        }
    }
}
