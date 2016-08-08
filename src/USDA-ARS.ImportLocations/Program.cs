using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
//using Umbraco.Core.Persistence;
using USDA_ARS.ImportLocations.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.ImportLocations
{
   class Program
   {
      public static string LocationConnectionString = ConfigurationManager.ConnectionStrings["arisPublicWebDbDSN"].ConnectionString;
      public static int MAIN_LOCATION_NODE_ID = 1111;

      static string LOG_FILE_TEXT = "";

      static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
      static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
      static DateTime TIME_STARTED = DateTime.MinValue;
      static DateTime TIME_ENDED = DateTime.MinValue;
      static List<ModeCodeNew> MODE_CODE_NEW_LIST = null;

      static void Main(string[] args)
      {
         if (args != null && args.Length >= 1)
         {
            if (args[0] == "import")
            {
               // Import the Locations data
               Import();
            }
            else if (args[0] == "delete")
            {
               Delete();
            }

            using (FileStream fs = File.Create("LOCATIONS_LOG_FILE.txt"))
            {
               // Add some text to file
               Byte[] fileText = new UTF8Encoding(true).GetBytes(LOG_FILE_TEXT);
               fs.Write(fileText, 0, fileText.Length);
            }
         }
         else
         {
            Console.WriteLine("A program attribute is required.");
            Console.WriteLine("There are 2 options:");
            Console.WriteLine("");
            Console.WriteLine("USDA-ARS.ImportLocations.exe import");
            Console.WriteLine("USDA-ARS.ImportLocations.exe delete");
         }
      }


      static void Import()
      {
         TIME_STARTED = DateTime.Now;

         AddLog("Getting New Mode Codes...");
         MODE_CODE_NEW_LIST = GetNewModeCodesAll();
         AddLog("Done. Count: " + MODE_CODE_NEW_LIST.Count);
         AddLog("");

         AddLog("-= UPDATING ARS HOME =-");
         ImportArsHomeInfo();

         AddLog("-= ADDING MAIN AREAS =-");
         AddLog("");
         List<Area> areaList = null;
         areaList = AddAllAreas();

         AddLog("");
         AddLog("");
         AddLog("");
         AddLog("-= ADDING CITIES =-");
         AddLog("");
         List<City> cityList = null;
         cityList = AddAllCities(areaList);


         AddLog("");
         AddLog("");
         AddLog("");
         AddLog("-= ADDING RESEARCH CENTERS =-");
         AddLog("");
         List<ResearchCenter> researchCenterList = null;
         researchCenterList = AddAllResearchCenters(cityList);


         AddLog("");
         AddLog("");
         AddLog("");
         AddLog("-= ADDING LABS =-");
         AddLog("");
         List<Lab> labList = null;
         labList = AddAllLabs(researchCenterList);



         AddLog("");
         AddLog("");
         AddLog("");
         AddLog("/// IMPORT COMPLETE ///");
         AddLog("");

         TIME_ENDED = DateTime.Now;

         TimeSpan timeLength = TIME_ENDED.Subtract(TIME_STARTED);

         AddLog("/// Time to complete: " + timeLength.ToString(@"hh") + " hours : " + timeLength.ToString(@"mm") + " minutes : " + timeLength.ToString(@"ss") + " seconds ///");
      }


      static void Delete()
      {

      }


      static void ImportArsHomeInfo()
      {
         int umbracoHomeId = 1075;
         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         AddLog("Updating ARS Home...");

         content.Id = umbracoHomeId;
         content.Name = "ARS Home";

         List<ApiProperty> properties = new List<ApiProperty>();

         // USED FOR ALL ARCHETYPE DATA TYPES
         var jsonSettings = new JsonSerializerSettings();
         jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

         properties.Add(new ApiProperty("modeCode", "00-00-00-00")); // Region mode code                                                                                            
         properties.Add(new ApiProperty("oldUrl", "/main/main.htm")); // current URL               
         properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).

         List<Link> resourcesList = new List<Link>();

         resourcesList.Add(new Link("Scientific Software / Models", "/research/software/", "icon-link"));
         resourcesList.Add(new Link("Scientific Manuscripts", "/research/publications/find-a-publication/", "icon-link"));
         resourcesList.Add(new Link("Research Success Stories", "/business/docs.htm?docid=769", "icon-link"));
         resourcesList.Add(new Link("Publications", "/services/docs.htm?docid=1279", "icon-link"));
         resourcesList.Add(new Link("Animal Welfare Ombudsman (Dr. Donald Knowles)", "mailto:animalwellbeing@ars.usda.gov?Subject=From ARS website", "icon-link"));
         resourcesList.Add(new Link("Animal Welfare Ombudsman Alternate (Dr. Susan Harper)", "mailto:susan.harper@ars.usda.gov?Subject=From ARS website", "icon-link"));
         resourcesList.Add(new Link("Factsheets", "/News/docs.htm?docid=128", "icon-link"));
         resourcesList.Add(new Link("Databases and Datasets", "/research/datasets/", "icon-link"));
         resourcesList.Add(new Link("Image Gallery", "/news-events/image-gallery/", "icon-link"));

         properties.Add(new ApiProperty("resources", JsonConvert.SerializeObject(resourcesList, Newtonsoft.Json.Formatting.None, jsonSettings)));


         content.Properties = properties;
         content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

         request.ContentList = new List<ApiContent>();
         request.ContentList.Add(content);

         AddLog("Saving area in Umbraco: '" + content.Name + "'...");

         ApiResponse responseBack = ApiCalls.PostData(request, "Post");

         if (responseBack != null)
         {
            if (responseBack.ContentList != null)
            {
               foreach (ApiContent responseContent in responseBack.ContentList)
               {
                  AddLog(" - Save success: " + responseContent.Success);

                  if (true == responseContent.Success)
                  {
                     AddLog(" - Content Umbraco Id: " + responseContent.Id);
                     AddLog(" - Node Name: " + responseContent.Name);
                     AddLog("");
                     AddLog(" - Getting Carousel & Sofware sub node folders...");

                     SubNodeFolders subNodeFolders = GetSubNodeFolders(responseContent.Id);

                     if (subNodeFolders != null)
                     {
                        // ADD CAROUSEL SLIDES
                        CreateCarouselSlides(subNodeFolders.CarouselNodeId, "00-00-00-00");


                        // ADD SOFTWARE
                        CreateSoftwareList(subNodeFolders.SoftwareNodeId, "00-00-00-00");
                     }
                  }
                  else
                  {
                     AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                  }

                  AddLog("");
               }
            }
         }

         AddLog("");

      }


      static List<Area> AddAllAreas()
      {
         List<Area> areaList = new List<Area>();

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         DataTable legacyAreasBeforeInsertion = new DataTable();
         DataTable legacyQuickLinksBeforeInsertion = new DataTable();
         DataTable legacyWebTrendsBeforeInsertion = new DataTable();

         legacyAreasBeforeInsertion = AddRetrieveLocationsDL.GetAllAreas();
         legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllAreasQuickLinks();
         legacyAreasBeforeInsertion = CompareTwoDataTables(legacyAreasBeforeInsertion, legacyQuickLinksBeforeInsertion);
         legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllAreasWebTrendsProfileIDs();
         legacyAreasBeforeInsertion = CompareTwoDataTables(legacyAreasBeforeInsertion, legacyWebTrendsBeforeInsertion);

         DataTable legacyCarouselSlidesBeforeInsertion = new DataTable();
         DataTable legacySoftwaresBeforeInsertion = new DataTable();

         //for (int i = 0; i < 1; i++)
         for (int i = 0; i < legacyAreasBeforeInsertion.Rows.Count; i++)
         {
            string completeModeCode = legacyAreasBeforeInsertion.Rows[i].Field<string>(0);
            string newModeCodeProperty = completeModeCode;
            string oldModeCodeProperty = "";

            DataTable legacyOldModeCodes = new DataTable();
            legacyOldModeCodes = AddRetrieveLocationsDL.GetAllOldModeCodesBasedOnNewModeCodes(newModeCodeProperty);
            if (legacyOldModeCodes.Rows.Count > 0)
            {
               oldModeCodeProperty = legacyOldModeCodes.Rows[0].Field<string>(1);
            }

            string areaName = legacyAreasBeforeInsertion.Rows[i].Field<string>(1);
            string quickLinks = CleanHtml.CleanUpHtml(legacyAreasBeforeInsertion.Rows[i].Field<string>(2), newModeCodeProperty);
            string webtrendsProfileID = legacyAreasBeforeInsertion.Rows[i].Field<string>(3);
            if (completeModeCode.Length < 11)
               completeModeCode = "0" + completeModeCode;

            if (false == string.IsNullOrEmpty(areaName))
            {
               AddLog("Creating new area: '" + areaName + "'...");

               content.Id = 0; // New page
               content.Name = areaName;
               content.ParentId = MAIN_LOCATION_NODE_ID;
               content.DocType = "Area";
               content.Template = "Region";

               List<ApiProperty> properties = new List<ApiProperty>();


               properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
               properties.Add(new ApiProperty("oldUrl", "/main/site_main.htm?modecode=" + newModeCodeProperty + "")); // current URL               
               properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
               properties.Add(new ApiProperty("quickLinks", quickLinks));
               properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));

               AddLog(" - Old Mode Codes: " + oldModeCodeProperty);

               properties.Add(new ApiProperty("oldModeCodes", oldModeCodeProperty)); // Separate old modes codes by a comma ,

               // ADD POPULAR TOPICS
               ApiProperty topicsApiProperty = CreatePopularTopicsList(newModeCodeProperty);
               if (topicsApiProperty != null)
               {
                  properties.Add(topicsApiProperty);
               }

               content.Properties = properties;
               content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

               request.ContentList = new List<ApiContent>();
               request.ContentList.Add(content);

               AddLog("Saving area in Umbraco: '" + content.Name + "'...");

               ApiResponse responseBack = ApiCalls.PostData(request, "Post");

               if (responseBack != null)
               {
                  if (responseBack.ContentList != null)
                  {
                     foreach (ApiContent responseContent in responseBack.ContentList)
                     {
                        AddLog(" - Save success: " + responseContent.Success);

                        if (true == responseContent.Success)
                        {
                           AddLog(" - Content Umbraco Id: " + responseContent.Id);
                           AddLog(" - Node Name: " + responseContent.Name);

                           areaList.Add(new Area() { UmbracoId = responseContent.Id, Name = responseContent.Name, ModeCode = responseContent.Properties[0].Value.ToString(), QuickLinks = responseContent.Properties[3].Value.ToString(), WebtrendsProfileID = responseContent.Properties[4].Value.ToString() });

                           SubNodeFolders subNodeFolders = GetSubNodeFolders(responseContent.Id);

                           if (subNodeFolders != null)
                           {
                              // ADD CAROUSEL SLIDES
                              CreateCarouselSlides(subNodeFolders.CarouselNodeId, newModeCodeProperty);


                              // ADD SOFTWARE
                              CreateSoftwareList(subNodeFolders.SoftwareNodeId, newModeCodeProperty);
                           }
                        }
                        else
                        {
                           AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                        }

                        AddLog("");
                     }
                  }
               }

               AddLog("");
            }
         }

         // If we have saved areas, lets published them.
         if (areaList != null && areaList.Any())
         {
            AddLog("Publishing Areas...");

            ApiRequest requestPublish = new ApiRequest();
            ApiContent contentPublish = new ApiContent();

            requestPublish.ApiKey = API_KEY;

            contentPublish.Id = MAIN_LOCATION_NODE_ID;

            requestPublish.ContentList = new List<ApiContent>();
            requestPublish.ContentList.Add(contentPublish);

            ApiResponse responseBack = ApiCalls.PostData(requestPublish, "PublishWithChildren");

            if (responseBack != null)
            {
               AddLog(" - Success: " + responseBack.Success);
               AddLog(" - Message: " + responseBack.Message);
            }
         }

         return areaList;

      }


      static List<City> AddAllCities(List<Area> areaList)
      {
         List<City> cityList = new List<City>();

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         if (areaList != null && areaList.Any())
         {
            foreach (Area areaItem in areaList)
            {
               AddLog("Adding Cities for: " + areaItem.Name);

               int parentAreaUmbracoId = areaItem.UmbracoId;
               string parentAreaModeCode = areaItem.ModeCode;

               if (parentAreaModeCode.Length < 11)
               {
                  parentAreaModeCode = "0" + parentAreaModeCode;
               }

               DataTable legacyCitiesBeforeInsertion = new DataTable();

               legacyCitiesBeforeInsertion = AddRetrieveLocationsDL.GetAllCities(Convert.ToInt32(parentAreaModeCode.Substring(0, 2)));

               // Get cities based on the mode code

               bool citiesSaved = false;

               for (int j = 0; j < legacyCitiesBeforeInsertion.Rows.Count; j++)
               {
                  string cityNameWithStateName = legacyCitiesBeforeInsertion.Rows[j].Field<string>(2);
                  string stateCode = legacyCitiesBeforeInsertion.Rows[j].Field<string>(3);
                  string cityNameWithStateCode = cityNameWithStateName.Split(',')[0] + '-' + stateCode;
                  string cityNameWithStateCodeWithComma = cityNameWithStateName.Split(',')[0].ToUpper() + ", " + stateCode;
                  string cityNameUpperOnly = cityNameWithStateName.Split(',')[0].ToUpper();

                  AddLog("Creating new city (" + areaItem.Name + "): '" + cityNameWithStateName + "'...");

                  request.ApiKey = API_KEY;
                  content.Id = 0; // New page
                                  // content.Name = "{City Name, State Code}";
                                  //content.ParentId = { The Umbraco Content ID for the AREA};
                  content.Name = cityNameWithStateName;
                  content.ParentId = parentAreaUmbracoId;
                  content.DocType = "City";
                  content.Template = "PeopleLocations";

                  List<ApiProperty> properties = new List<ApiProperty>();
                  string newModeCodeProperty = legacyCitiesBeforeInsertion.Rows[j].Field<string>(1);
                  string oldModeCodeProperty = "";

                  properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // mode code                                                                                            
                  properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                  properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                                                                //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                  properties.Add(new ApiProperty("state", stateCode)); // For example: NY (2 letter state code)
                  properties.Add(new ApiProperty("navigationTitle", cityNameWithStateCodeWithComma)); // All CAPS - For example: GENEVA, NY
                  properties.Add(new ApiProperty("umbracoUrlName", cityNameWithStateCode.ToLower()));
                  //input is city name in upper case removing state code
                  string usaJobsLocationIDproperty = "";
                  DataTable legacyUsaJobsLocationCities = new DataTable();

                  legacyUsaJobsLocationCities = AddRetrieveLocationsDL.GetAllJobLocationIdsBasedOnCityName(cityNameUpperOnly);
                  if (legacyUsaJobsLocationCities.Rows.Count > 0)
                  {
                     usaJobsLocationIDproperty = legacyUsaJobsLocationCities.Rows[0].Field<string>(0);
                  }

                  properties.Add(new ApiProperty("usajobsLocationID", usaJobsLocationIDproperty)); // USDAJOBS Location ID

                  DataTable legacyOldModeCodes = new DataTable();
                  legacyOldModeCodes = AddRetrieveLocationsDL.GetAllOldModeCodesBasedOnNewModeCodes(newModeCodeProperty);
                  if (legacyOldModeCodes.Rows.Count > 0)
                  {
                     oldModeCodeProperty = legacyOldModeCodes.Rows[0].Field<string>(1);
                  }
                  properties.Add(new ApiProperty("oldModeCodes", oldModeCodeProperty)); // Separate old modes codes by a comma ,

                  AddLog(" - Mode Code: " + newModeCodeProperty);
                  AddLog(" - USA JOBS Location ID: " + usaJobsLocationIDproperty);

                  //properties.Add(new ApiProperty("modeCode", "80-10-00-00")); // Region mode code
                  //properties.Add(new ApiProperty("oldUrl", "")); // Leave blank since there is no city page on the website.
                  //properties.Add(new ApiProperty("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
                  //properties.Add(new ApiProperty("state", "{State Code}")); // For example: NY (2 letter state code)
                  //properties.Add(new ApiProperty("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

                  content.Properties = properties;

                  content.Save = 1; // 1=Saved, 2=Save And Publish

                  request.ContentList = new List<ApiContent>();
                  request.ContentList.Add(content);

                  AddLog("Saving city in Umbraco: '" + content.Name + "'...");

                  ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                  if (responseBack != null)
                  {
                     if (responseBack.ContentList != null)
                     {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                           AddLog(" - Save success: " + responseContent.Success);

                           if (true == responseContent.Success)
                           {
                              AddLog(" - Content Umbraco Id: " + responseContent.Id);
                              AddLog(" - Node Name: " + responseContent.Name);

                              cityList.Add(new City() { UmbracoId = responseContent.Id, Name = responseContent.Name, ModeCode = responseContent.Properties[0].Value.ToString(), StateCode = stateCode });

                              citiesSaved = true;
                           }
                           else
                           {
                              AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                           }

                           AddLog("");
                        }
                     }
                  }

                  AddLog("");


               }

               // If we have saved cities, lets published them.
               if (true == citiesSaved)
               {
                  AddLog("Publishing cities for '" + areaItem.Name + "'...");

                  ApiRequest requestPublish = new ApiRequest();
                  ApiContent contentPublish = new ApiContent();

                  requestPublish.ApiKey = API_KEY;

                  contentPublish.Id = areaItem.UmbracoId;

                  requestPublish.ContentList = new List<ApiContent>();
                  requestPublish.ContentList.Add(contentPublish);

                  ApiResponse responseBackPublish = ApiCalls.PostData(requestPublish, "PublishWithChildren");

                  if (responseBackPublish != null)
                  {
                     AddLog(" - Success: " + responseBackPublish.Success);
                     AddLog(" - Message: " + responseBackPublish.Message);
                  }
               }

            }
         }

         return cityList;
      }


      static List<ResearchCenter> AddAllResearchCenters(List<City> cityList)
      {
         List<ResearchCenter> researchCenterList = new List<ResearchCenter>();

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         if (cityList != null && cityList.Any())
         {
            foreach (City cityItem in cityList)
            {
               AddLog("Adding Research Centers for: " + cityItem.Name);

               int parentCityUmbracoId = cityItem.UmbracoId;
               string parentLocationModeCode = cityItem.ModeCode;

               if (parentLocationModeCode.Length < 11)
               {
                  parentLocationModeCode = "0" + parentLocationModeCode;
               }

               int areaModeCode = Convert.ToInt32(parentLocationModeCode.Substring(0, 2));
               int cityModeCode = Convert.ToInt32(parentLocationModeCode.Substring(3, 2));

               DataTable legacyResearchUnitsBeforeInsertion = new DataTable();
               DataTable legacyQuickLinksBeforeInsertion = new DataTable();
               DataTable legacyWebTrendsBeforeInsertion = new DataTable();
               DataTable legacyCarouselSlidesBeforeInsertion = new DataTable();
               DataTable legacySoftwaresBeforeInsertion = new DataTable();

               AddLog(" - Getting Research Centers...");
               legacyResearchUnitsBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnits(areaModeCode, cityModeCode);

               AddLog(" - Getting Research Centers quick links...");
               legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnitsQuickLinks(areaModeCode, cityModeCode);

               legacyResearchUnitsBeforeInsertion = CompareTwoDataTables(legacyResearchUnitsBeforeInsertion, legacyQuickLinksBeforeInsertion);

               AddLog(" - Getting Research Centers web trends...");
               legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnitsWebTrends(areaModeCode, cityModeCode);

               legacyResearchUnitsBeforeInsertion = CompareTwoDataTables(legacyResearchUnitsBeforeInsertion, legacyWebTrendsBeforeInsertion);

               AddLog("");

               bool researchCenterSaved = false;

               for (int j = 0; j < legacyResearchUnitsBeforeInsertion.Rows.Count; j++)
               {
                  string newModeCodeProperty = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(2);
                  string oldModeCodeProperty = "";

                  DataTable legacyOldModeCodes = new DataTable();
                  legacyOldModeCodes = AddRetrieveLocationsDL.GetAllOldModeCodesBasedOnNewModeCodes(newModeCodeProperty);
                  if (legacyOldModeCodes.Rows.Count > 0)
                  {
                     oldModeCodeProperty = legacyOldModeCodes.Rows[0].Field<string>(1);
                  }

                  string rCName = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(3);
                  string quickLinks = CleanHtml.CleanUpHtml(legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(4), newModeCodeProperty);
                  string webtrendsProfileID = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(5);

                  if (false == string.IsNullOrWhiteSpace(rCName))
                  {
                     AddLog("Creating new research center (" + cityItem.Name + "): '" + rCName + "'...");

                     request.ApiKey = API_KEY;

                     content.Id = 0; // New page
                                     // content.Name = "{City Name, State Code}";
                                     //content.ParentId = { The Umbraco Content ID for the AREA};
                     content.Name = rCName;
                     content.ParentId = parentCityUmbracoId;
                     content.DocType = "ResearchUnit";
                     content.Template = "ResearchUnit"; // Leave blank

                     List<ApiProperty> properties = new List<ApiProperty>();


                     properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                     properties.Add(new ApiProperty("oldUrl", "/main/site_main.htm?modecode=" + newModeCodeProperty + "")); // current URL               
                     properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                     properties.Add(new ApiProperty("quickLinks", quickLinks));                                  //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                     properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));



                     properties.Add(new ApiProperty("oldModeCodes", oldModeCodeProperty)); // Separate old modes codes by a comma ,

                     content.Properties = properties;

                     content.Save = 1; // 1=Saved, 2=Save And Publish

                     request.ContentList = new List<ApiContent>();
                     request.ContentList.Add(content);

                     AddLog("Saving new research center (" + cityItem.Name + "): '" + rCName + "'...");

                     ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                     if (responseBack != null)
                     {
                        if (responseBack.ContentList != null)
                        {
                           foreach (ApiContent responseContent in responseBack.ContentList)
                           {
                              AddLog(" - Save success: " + responseContent.Success);

                              if (true == responseContent.Success)
                              {
                                 AddLog(" - Content Umbraco Id: " + responseContent.Id);
                                 AddLog(" - Node Name: " + responseContent.Name);

                                 researchCenterList.Add(new ResearchCenter() { UmbracoId = responseContent.Id, Name = responseContent.Name, ModeCode = responseContent.Properties[0].Value.ToString(), QuickLinks = quickLinks, WebtrendsProfileID = webtrendsProfileID });

                                 SubNodeFolders subNodeFolders = GetSubNodeFolders(responseContent.Id);

                                 if (subNodeFolders != null)
                                 {
                                    // ADD CAROUSEL SLIDES
                                    CreateCarouselSlides(subNodeFolders.CarouselNodeId, newModeCodeProperty);


                                    // ADD SOFTWARE
                                    CreateSoftwareList(subNodeFolders.SoftwareNodeId, newModeCodeProperty);
                                 }

                                 researchCenterSaved = true;
                              }
                              else
                              {
                                 AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                              }

                              AddLog("");
                           }
                        }
                     }

                     AddLog("");
                  }
               }


               // If we have saved research centers, lets published them.
               if (true == researchCenterSaved)
               {
                  AddLog("Publishing research centers for '" + cityItem.Name + "'...");

                  ApiRequest requestPublish = new ApiRequest();
                  ApiContent contentPublish = new ApiContent();

                  requestPublish.ApiKey = API_KEY;

                  contentPublish.Id = cityItem.UmbracoId;

                  requestPublish.ContentList = new List<ApiContent>();
                  requestPublish.ContentList.Add(contentPublish);

                  ApiResponse responseBackPublish = ApiCalls.PostData(requestPublish, "PublishWithChildren");

                  if (responseBackPublish != null)
                  {
                     AddLog(" - Success: " + responseBackPublish.Success);
                     AddLog(" - Message: " + responseBackPublish.Message);
                  }
               }




            }
         }

         return researchCenterList;
      }


      static List<Lab> AddAllLabs(List<ResearchCenter> researchCenterList)
      {
         List<Lab> labList = new List<Lab>();

         ApiRequest request = new ApiRequest();
         ApiContent content = new ApiContent();

         request.ApiKey = API_KEY;

         if (researchCenterList != null && researchCenterList.Any())
         {
            foreach (ResearchCenter researchCenterItem in researchCenterList)
            {
               AddLog("Adding Lab for: " + researchCenterItem.Name);

               int parentResearchUnitUmbracoId = researchCenterItem.UmbracoId;
               string parentLocationModeCode = researchCenterItem.ModeCode;

               if (parentLocationModeCode.Length < 11)
               {
                  parentLocationModeCode = "0" + parentLocationModeCode;
               }

               int areaModeCode = Convert.ToInt32(parentLocationModeCode.Substring(0, 2));
               int cityModeCode = Convert.ToInt32(parentLocationModeCode.Substring(3, 2));
               int rcModeCode = Convert.ToInt32(parentLocationModeCode.Substring(6, 2));

               DataTable legacyLabsBeforeInsertion = new DataTable();

               DataTable legacyQuickLinksBeforeInsertion = new DataTable();
               DataTable legacyWebTrendsBeforeInsertion = new DataTable();
               DataTable legacyCarouselSlidesBeforeInsertion = new DataTable();
               DataTable legacySoftwaresBeforeInsertion = new DataTable();


               AddLog(" - Getting Labs...");
               legacyLabsBeforeInsertion = AddRetrieveLocationsDL.GetAllLabs(areaModeCode, cityModeCode, rcModeCode);

               AddLog(" - Getting Labs quick links...");
               legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllLabsQuickLinks(areaModeCode, cityModeCode, rcModeCode);
               legacyLabsBeforeInsertion = CompareTwoDataTables(legacyLabsBeforeInsertion, legacyQuickLinksBeforeInsertion);

               AddLog(" - Getting Labs web trends...");
               legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllLabsWebTrendsProfileIDs(areaModeCode, cityModeCode, rcModeCode); ;
               legacyLabsBeforeInsertion = CompareTwoDataTables(legacyLabsBeforeInsertion, legacyWebTrendsBeforeInsertion);

               AddLog("");

               bool labSaved = false;

               for (int j = 0; j < legacyLabsBeforeInsertion.Rows.Count; j++)
               {
                  string labName = legacyLabsBeforeInsertion.Rows[j].Field<string>(4);

                  if (false == string.IsNullOrWhiteSpace(labName))
                  {

                     string newModeCodeProperty = legacyLabsBeforeInsertion.Rows[j].Field<string>(3);
                     string oldModeCodeProperty = "";

                     System.Data.DataTable legacyOldModeCodes = new System.Data.DataTable();
                     legacyOldModeCodes = AddRetrieveLocationsDL.GetAllOldModeCodesBasedOnNewModeCodes(newModeCodeProperty);
                     if (legacyOldModeCodes.Rows.Count > 0)
                     {
                        oldModeCodeProperty = legacyOldModeCodes.Rows[0].Field<string>(1);
                     }

                     string quickLinks = CleanHtml.CleanUpHtml(legacyLabsBeforeInsertion.Rows[j].Field<string>(5), newModeCodeProperty);
                     string webtrendsProfileID = legacyLabsBeforeInsertion.Rows[j].Field<string>(6);



                     request.ApiKey = API_KEY;
                     content.Id = 0; // New page
                                     // content.Name = "{City Name, State Code}";
                                     //content.ParentId = { The Umbraco Content ID for the AREA};
                     content.Name = labName;
                     content.ParentId = parentResearchUnitUmbracoId;
                     content.DocType = "ResearchUnit";
                     content.Template = "ResearchUnit";

                     List<ApiProperty> properties = new List<ApiProperty>();


                     properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // mode code                                                                                            
                     properties.Add(new ApiProperty("oldUrl", "/main/site_main.htm?modecode=" + newModeCodeProperty + "")); // current URL               
                     properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                     properties.Add(new ApiProperty("quickLinks", quickLinks));                                              //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                     properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));


                     properties.Add(new ApiProperty("oldModeCodes", oldModeCodeProperty)); // Separate old modes codes by a comma ,
                     
                     content.Properties = properties;

                     content.Save = 1; // 1=Saved, 2=Save And Publish

                     request.ContentList = new List<ApiContent>();
                     request.ContentList.Add(content);

                     AddLog("Saving new lab (" + researchCenterItem.Name + "): '" + labName + "'...");

                     ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                     if (responseBack != null)
                     {
                        if (responseBack.ContentList != null)
                        {
                           foreach (ApiContent responseContent in responseBack.ContentList)
                           {
                              AddLog(" - Save success: " + responseContent.Success);

                              if (true == responseContent.Success)
                              {
                                 AddLog(" - Content Umbraco Id: " + responseContent.Id);
                                 AddLog(" - Node Name: " + responseContent.Name);

                                 labList.Add(new Lab() { UmbracoId = responseContent.Id, Name = responseContent.Name, ModeCode = responseContent.Properties[0].Value.ToString(), QuickLinks = quickLinks, WebtrendsProfileID = webtrendsProfileID });

                                 SubNodeFolders subNodeFolders = GetSubNodeFolders(responseContent.Id);

                                 if (subNodeFolders != null)
                                 {
                                    // ADD CAROUSEL SLIDES
                                    CreateCarouselSlides(subNodeFolders.CarouselNodeId, newModeCodeProperty);


                                    // ADD SOFTWARE
                                    CreateSoftwareList(subNodeFolders.SoftwareNodeId, newModeCodeProperty);
                                 }

                                 labSaved = true;
                              }
                              else
                              {
                                 AddLog(" - !ERROR! Fail Message: " + responseContent.Message);
                              }

                              AddLog("");
                           }
                        }
                     }

                     AddLog("");
                  }
               }


               // If we have saved research centers, lets published them.
               if (true == labSaved)
               {
                  AddLog("Publishing labs for '" + researchCenterItem.Name + "'...");

                  ApiRequest requestPublish = new ApiRequest();
                  ApiContent contentPublish = new ApiContent();

                  requestPublish.ApiKey = API_KEY;

                  contentPublish.Id = researchCenterItem.UmbracoId;

                  requestPublish.ContentList = new List<ApiContent>();
                  requestPublish.ContentList.Add(contentPublish);

                  ApiResponse responseBackPublish = ApiCalls.PostData(requestPublish, "PublishWithChildren");

                  if (responseBackPublish != null)
                  {
                     AddLog(" - Success: " + responseBackPublish.Success);
                     AddLog(" - Message: " + responseBackPublish.Message);
                  }
               }
            }
         }

         return labList;
      }


      static DataTable CompareTwoDataTables(DataTable dataTable1, DataTable dataTable2)
      {
         DataTable dataTable3 = new DataTable();
         dataTable1.PrimaryKey = new DataColumn[] { dataTable1.Columns["Mode Code"] };
         dataTable2.PrimaryKey = new DataColumn[] { dataTable2.Columns["Mode Code"] };
         dataTable3 = dataTable1.Copy();
         dataTable3.Merge(dataTable2, false, MissingSchemaAction.Add);
         dataTable3.AcceptChanges();

         return dataTable3;
      }


      static List<string> ReadFromTextfile(int softwareId)
      {
         string current_path = "getFileNamesForSoftwareIdFolders.txt";

         string[] lines = File.ReadAllLines(current_path);

         List<string> myCollection = new List<string>();

         for (int i = 0; i < lines.Length; i++)
         {
            if (lines[i].StartsWith(softwareId.ToString()+"\\"))
            {
               myCollection.Add(lines[i]);
            }
         }

         return myCollection;
      }


      static SubNodeFolders GetSubNodeFolders(int locationId)
      {
         SubNodeFolders subNodeFolders = null;

         ApiResponse response = GetCalls.GetNodeByUmbracoId(locationId);

         if (response != null && response.ContentList != null && response.ContentList.Any())
         {
            subNodeFolders = new SubNodeFolders();

            subNodeFolders.LocationNodeId = locationId;

            if (response.ContentList[0].ChildContentList != null && response.ContentList[0].ChildContentList.Any())
            {
               ApiContent carouselNode = response.ContentList[0].ChildContentList.Where(p => p.DocType == "SiteCarouselFolder").FirstOrDefault();

               if (carouselNode != null)
               {
                  subNodeFolders.CarouselNodeId = carouselNode.Id;
               }

               ApiContent softwareNode = response.ContentList[0].ChildContentList.Where(p => p.DocType == "SiteSoftwareFolder").FirstOrDefault();

               if (softwareNode != null)
               {
                  subNodeFolders.SoftwareNodeId = softwareNode.Id;
               }
            }
            else
            {
               AddLog("!!! Could not find sub-nodes: " + locationId);
            }

         }
         else
         {
            AddLog("!!! COULD NOT GET NODE FROM UMBRACO: " + locationId);
         }


         return subNodeFolders;
      }


      static void CreateCarouselSlides(int carouselFolderNodeId, string modeCode)
      {
         DataTable legacyCarouselSlidesBeforeInsertion = new DataTable();
         bool publishLater = false;

         AddLog(" - Getting carousel sides for (" + modeCode + ")...");

         legacyCarouselSlidesBeforeInsertion = AddRetrieveLocationsDL.GetAllCarouselSlidesBasedOnModeCode(modeCode);

         AddLog(" - Carousel Slide Count: " + legacyCarouselSlidesBeforeInsertion.Rows.Count);

         for (int legacyCarouselSlidesRowId = 0; legacyCarouselSlidesRowId < legacyCarouselSlidesBeforeInsertion.Rows.Count; legacyCarouselSlidesRowId++)
         {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

            string slideName = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(5);
            string slideText = CleanHtml.CleanUpHtml(legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(4), modeCode);
            string slideAltText = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(3);
            string slideURL = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(2);
            string slideImage = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(1);
            string slideImageSP2 = "/ARSUserFiles/" + "" + modeCode.Replace("-", "") + "/images/photoCarousel/" + slideImage;
            string slideFilePath = null;

            if (slideURL.ToLower().EndsWith(".pdf") || slideURL.ToLower().EndsWith(".doc") || slideURL.ToLower().EndsWith(".xls"))
            {
               slideFilePath = slideURL;
            }

            slideName = CleanHtml.ReplaceUnicodeText(slideName);

            if (true == string.IsNullOrWhiteSpace(slideName))
            {
               slideName = "Slide";
            }

            content.Id = 0;
            content.ParentId = carouselFolderNodeId;

            content.Name = slideName;


            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("slideImage", slideImageSP2));
            properties.Add(new ApiProperty("slideText", "<p>" + slideText + "</p>"));
            properties.Add(new ApiProperty("slideAltText", slideAltText));

            // if slide file path is not empty, set it
            if (false == string.IsNullOrEmpty(slideFilePath))
            {
               slideFilePath = CleanHtml.CleanUpHtml(slideFilePath);

               properties.Add(new ApiProperty("slideFile", slideFilePath));
               properties.Add(new ApiProperty("slideUrl", ""));
            }
            else // Set the URL instead.
            {
               if (false == string.IsNullOrEmpty(slideURL))
               {
                  slideURL = CleanHtml.CleanUpHtml(slideURL);
                  Link linkSlide = new Link(slideURL, slideURL, ""); // set the url path

                  properties.Add(new ApiProperty("slideUrl", "[" + JsonConvert.SerializeObject(linkSlide, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));
               }
               else
               {
                  properties.Add(new ApiProperty("slideUrl", ""));
               }

               properties.Add(new ApiProperty("slideFile", ""));
            }

            content.Properties = properties;
            content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish
            content.DocType = "CarouselSlide";
            content.Template = "";

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            AddLog("Saving slide in Umbraco: '" + content.Name + "'...");

            ApiResponse responseBack = ApiCalls.PostData(request, "Post");

            if (responseBack != null && responseBack.ContentList != null && responseBack.ContentList.Any())
            {
               if (true == responseBack.ContentList[0].Success)
               {
                  AddLog(" - Saved: " + responseBack.ContentList[0].Name);
                  publishLater = true;
               }
               else
               {
                  AddLog(" !!! NOT SAVED !!!: " + responseBack.ContentList[0].Message);
               }
            }
            else
            {
               if (responseBack == null || responseBack.ContentList != null || false == responseBack.ContentList.Any())
               {
                  AddLog(" !!! NOT Saved");
               }
               else
               {
                  AddLog(" !!! NOT Saved!");
               }

            }
         }

         if (true == publishLater)
         {
            PublishChildrenNodes(carouselFolderNodeId);
         }
      }


      static void CreateSoftwareList(int softwareFolderNodeId, string modeCode)
      {
         DataTable legacySoftwaresBeforeInsertion = new DataTable();
         bool publishLater = false;

         ////softwares
         AddLog(" - Getting software for (" + modeCode + ")...");


         legacySoftwaresBeforeInsertion = AddRetrieveLocationsDL.GetAllSoftwaresBasedOnModeCode(modeCode);

         AddLog(" - Software Count: " + legacySoftwaresBeforeInsertion.Rows.Count);

         for (int legacySoftwaresRowId = 0; legacySoftwaresRowId < legacySoftwaresBeforeInsertion.Rows.Count; legacySoftwaresRowId++)
         {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

            string softwareID = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<int>(1).ToString();
            string title = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(2);
            string recipients = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(3);
            string shortBlurb = CleanHtml.CleanUpHtml(legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(4), modeCode);
            string info = CleanHtml.CleanUpHtml(legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(5), modeCode);

            if (true == string.IsNullOrWhiteSpace(title))
            {
               title = "Software";
            }

            content.Id = 0;
            content.ParentId = softwareFolderNodeId;
            content.DocType = "SoftwareItem";
            content.Template = "";

            content.Name = title;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("softwareID", softwareID));
            properties.Add(new ApiProperty("recipients", recipients.Replace(" ", "")));
            properties.Add(new ApiProperty("shortBlurb", shortBlurb));
            properties.Add(new ApiProperty("information", info));

            string fileListJson = "";
            List <string> filePathSP2List = new List<string>();
            filePathSP2List = ReadFromTextfile(Convert.ToInt32(softwareID));

            //get files from software id folder
            if (filePathSP2List != null && filePathSP2List.Count > 0)
            {
               ApiArchetype softwareFilesList = new ApiArchetype();

               softwareFilesList.Fieldsets = new List<Fieldset>();

               // LOOP Through the list of files
               {
                  for (int filePathSP2ListRowId = 0; filePathSP2ListRowId < filePathSP2List.Count; filePathSP2ListRowId++)
                  {
                     Fieldset fieldsetFiles = new Fieldset();

                     fieldsetFiles.Alias = "softwareDownloads";
                     fieldsetFiles.Disabled = false;
                     fieldsetFiles.Id = Guid.NewGuid();
                     fieldsetFiles.Properties = new List<Property>();
                     string filePathSP2 = "/ARSUserFiles/" + "" + modeCode.Replace("-", "") + "/software/" + filePathSP2List[filePathSP2ListRowId].Replace("\\", "/");
                     fieldsetFiles.Properties.Add(new Property("file", filePathSP2)); // set the file path
                     softwareFilesList.Fieldsets.Add(fieldsetFiles);
                  }

                  fileListJson = JsonConvert.SerializeObject(softwareFilesList, Formatting.None, jsonSettings);
               }
            }

            if (filePathSP2List != null && filePathSP2List.Count > 0)
            {
               properties.Add(new ApiProperty("fileDownloads", fileListJson));

               content.Properties = properties;
               content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

               request.ContentList = new List<ApiContent>();
               request.ContentList.Add(content);

               AddLog("Saving software in Umbraco: '" + content.Name + "'...");

               ApiResponse responseBack = ApiCalls.PostData(request, "Post");

               if (responseBack != null && responseBack.ContentList != null && responseBack.ContentList.Any())
               {
                  if (true == responseBack.ContentList[0].Success)
                  {
                     AddLog(" - Saved: " + responseBack.ContentList[0].Name);
                     publishLater = true;
                  }
                  else
                  {
                     AddLog(" !!! NOT SAVED !!!: " + responseBack.ContentList[0].Message);
                  }
               }
               else
               {
                  if (responseBack == null || responseBack.ContentList != null || false == responseBack.ContentList.Any())
                  {
                     AddLog(" !!! NOT Saved");
                  }
                  else
                  {
                     AddLog(" !!! NOT Saved!");
                  }
               }
            }
            else
            {
               AddLog(" - Software bypassed because no files were linked.");
            }
         }

         if (true == publishLater)
         {
            PublishChildrenNodes(softwareFolderNodeId);
         }
      }


      static ApiProperty CreatePopularTopicsList(string modeCode)
      {
         ApiProperty topicProperty = null;
         string topicString = null;

         List<PopularLink> linkList = PopularTopics.GetPopularTopicsByModeCode(ModeCodes.ModeCodeNoDashes(modeCode));

         ////popular topics
         AddLog(" - Getting topics for (" + modeCode + ")...");

         if (linkList != null && linkList.Any())
         {
            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();


            // ADD POPULAR TOPICS
            ApiArchetype popularTopics = new ApiArchetype();

            popularTopics.Fieldsets = new List<Fieldset>();

            // Here is where you would loop through each Popular Topics Link
            // LOOP START
            foreach (PopularLink popLink in linkList)
            {
               Fieldset fieldset = new Fieldset();

               fieldset.Alias = "popularTopics";
               fieldset.Disabled = false;
               fieldset.Id = Guid.NewGuid();
               fieldset.Properties = new List<Property>();
               fieldset.Properties.Add(new Property("label", popLink.Label)); // set the label name

               string url = popLink.URL;

               if (false == string.IsNullOrWhiteSpace(url))
               {
                  url = CleanHtml.CleanUpHtml(url);
               }

               Link link = new Link(url, url, ""); // set the url path
               fieldset.Properties.Add(new Property("link", "[" + JsonConvert.SerializeObject(link, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));

               popularTopics.Fieldsets.Add(fieldset);
            }
            // LOOP END

            if (popularTopics != null && popularTopics.Fieldsets != null && popularTopics.Fieldsets.Any())
            {
               topicString = JsonConvert.SerializeObject(popularTopics, Newtonsoft.Json.Formatting.None, jsonSettings);
            }
         }

         if (false == string.IsNullOrEmpty(topicString))
         {
            topicProperty = new ApiProperty("popularTopics", topicString);
         }
         else
         {
            topicProperty = new ApiProperty("popularTopics", "");
         }

         AddLog(" - Popular Topics added: " + linkList.Count);

         return topicProperty;
      }


      static void PublishChildrenNodes(int parentId)
      {
         AddLog("Publishing nodes...");

         ApiRequest requestPublish = new ApiRequest();
         ApiContent contentPublish = new ApiContent();

         requestPublish.ApiKey = API_KEY;

         contentPublish.Id = parentId;

         requestPublish.ContentList = new List<ApiContent>();
         requestPublish.ContentList.Add(contentPublish);

         ApiResponse responseBack = ApiCalls.PostData(requestPublish, "PublishWithChildren");

         if (responseBack != null)
         {
            AddLog(" - Success: " + responseBack.Success);
            AddLog(" - Message: " + responseBack.Message);
         }
      }


      static List<ModeCodeNew> GetNewModeCodesAll()
      {
         List<ModeCodeNew> modeCodeNewList = new List<ModeCodeNew>();

         //var db = new Database("arisPublicWebDbDSN");

         //string sql = @"SELECT * FROM NewModecodes";

         //modeCodeNewList = db.Query<ModeCodeNew>(sql).ToList();

         return modeCodeNewList;
      }


      static void AddLog(string line)
      {
         Debug.WriteLine(line);
         Console.WriteLine(line);
         LOG_FILE_TEXT += line + "\r\n";
      }

   }
}
