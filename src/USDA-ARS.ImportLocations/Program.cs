﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using USDA_ARS.ImportLocations.Models;
using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
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

                using (FileStream fs = File.Create("LOG_FILE.txt"))
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

                    // ADD CAROUSEL
                    ApiProperty carouselApiProperty = CreateCarouselSlides(newModeCodeProperty);
                    if (carouselApiProperty != null)
                    {
                        properties.Add(carouselApiProperty);
                    }

                    // ADD SOFTWARE
                    ApiProperty softwareApiProperty = CreateSoftwareList(newModeCodeProperty);
                    if (softwareApiProperty != null)
                    {
                        properties.Add(softwareApiProperty);
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

                            // ADD CAROUSEL
                            ApiProperty carouselApiProperty = CreateCarouselSlides(newModeCodeProperty);
                            if (carouselApiProperty != null)
                            {
                                properties.Add(carouselApiProperty);
                            }

                            // ADD SOFTWARE
                            ApiProperty softwareApiProperty = CreateSoftwareList(newModeCodeProperty);
                            if (softwareApiProperty != null)
                            {
                                properties.Add(softwareApiProperty);
                            }


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

                            // ADD CAROUSEL
                            ApiProperty carouselApiProperty = CreateCarouselSlides(newModeCodeProperty);
                            if (carouselApiProperty != null)
                            {
                                properties.Add(carouselApiProperty);
                            }

                            // ADD SOFTWARE
                            ApiProperty softwareApiProperty = CreateSoftwareList(newModeCodeProperty);
                            if (softwareApiProperty != null)
                            {
                                properties.Add(softwareApiProperty);
                            }


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
                if (lines[i].Contains(softwareId.ToString()))
                {
                    myCollection.Add(lines[i]);
                }
            }

            return myCollection;
        }


        static ApiProperty CreateCarouselSlides(string modeCode)
        {
            ApiProperty carouselProperty = null;

            DataTable legacyCarouselSlidesBeforeInsertion = new DataTable();

            AddLog(" - Getting carousel sides for (" + modeCode + ")...");

            legacyCarouselSlidesBeforeInsertion = AddRetrieveLocationsDL.GetAllCarouselSlidesBasedOnModeCode(modeCode);

            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();


            // ADD CAROUSEL
            ApiArchetype carouselSlide = new ApiArchetype();

            carouselSlide.Fieldsets = new List<Fieldset>();
            string slideJson = null;


            for (int legacyCarouselSlidesRowId = 0; legacyCarouselSlidesRowId < legacyCarouselSlidesBeforeInsertion.Rows.Count; legacyCarouselSlidesRowId++)
            {
                Fieldset fieldsetCar = new Fieldset();
                // Here is where you would loop through each Carousel Slide Link
                // LOOP START
                string slideName = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(5);
                string slideText = CleanHtml.CleanUpHtml(legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(4), modeCode);
                string slideAltText = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(3);
                string slideURL = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(2);
                string slideImage = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(1);
                string slideImageSP2 = "/ARSUserFiles/" + "" + modeCode.Replace("-", "") + "/photoCarousel/" + slideImage;
                string slideFilePath = null;
                if (slideURL.Contains("/ARSUserFiles/"))
                {
                    slideFilePath = slideImageSP2; // If a slide links to a file instead of a page, set it here.
                }

                slideName = CleanHtml.ReplaceUnicodeText(slideName);

                fieldsetCar.Alias = "slide";
                fieldsetCar.Disabled = false;
                fieldsetCar.Id = new Guid();
                fieldsetCar.Properties = new List<Property>();
                fieldsetCar.Properties.Add(new Property("slideName", slideName)); // set the slide name
                fieldsetCar.Properties.Add(new Property("slideImage", slideImageSP2)); // set the slide image path
                fieldsetCar.Properties.Add(new Property("slideText", "<p>" + slideText + "</p>")); // set the slide html text
                fieldsetCar.Properties.Add(new Property("slideAltText", slideAltText)); // set the slide alt text

                // if slide file path is not empty, set it
                if (false == string.IsNullOrEmpty(slideFilePath))
                {
                    fieldsetCar.Properties.Add(new Property("slideFile", slideFilePath)); // set the slide file path
                    fieldsetCar.Properties.Add(new Property("slideUrl", "")); // set the slide url to empty
                }
                else // Set the URL instead.
                {
                    if (slideURL.Contains("ars.usda.gov"))
                    {
                        slideURL = Regex.Replace(slideURL, @"http(s)*://www\.ars\.usda\.gov", "");
                        slideURL = Regex.Replace(slideURL, @"http(s)*://ars\.usda\.gov", "");

                        Link linkSlide = new Link(slideURL, slideURL, ""); // set the url path
                        fieldsetCar.Properties.Add(new Property("link", "[" + JsonConvert.SerializeObject(linkSlide, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));
                    }
                    else
                    {
                        if (false == string.IsNullOrEmpty(slideURL))
                        {
                            fieldsetCar.Properties.Add(new Property("slideUrl", slideURL));
                        }
                        else
                        {
                            fieldsetCar.Properties.Add(new Property("slideUrl", ""));
                        }
                    }

                    fieldsetCar.Properties.Add(new Property("slideFile", "")); // set the slide alt text
                }

                carouselSlide.Fieldsets.Add(fieldsetCar);
                // LOOP END
            }

            if (carouselSlide != null && carouselSlide.Fieldsets != null && carouselSlide.Fieldsets.Count > 0)
            {
                slideJson = JsonConvert.SerializeObject(carouselSlide, Newtonsoft.Json.Formatting.None, jsonSettings);
            }

            // Last, we set the ApiProperty for "carouselSlide"
            if (false == string.IsNullOrEmpty(slideJson))
            {
                carouselProperty = new ApiProperty("carouselSlides", slideJson);
            }
            else
            {
                carouselProperty = new ApiProperty("carouselSlides", "");
            }

            AddLog(" - Slides added: " + legacyCarouselSlidesBeforeInsertion.Rows.Count);

            return carouselProperty;
        }


        static ApiProperty CreateSoftwareList(string modeCode)
        {
            ApiProperty softwareProperty = null;

            DataTable legacySoftwaresBeforeInsertion = new DataTable();

            ////softwares
            AddLog(" - Getting software for (" + modeCode + ")...");
            legacySoftwaresBeforeInsertion = AddRetrieveLocationsDL.GetAllSoftwaresBasedOnModeCode(modeCode);


            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettingsForSoftware = new JsonSerializerSettings();
            jsonSettingsForSoftware.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

            // ADD software
            ApiArchetype softwareItem = new ApiArchetype();
            softwareItem.Fieldsets = new List<Fieldset>();
            string filePackageJson = null;

            // Here is where you would loop through each Carousel Slide Link
            // LOOP START
            for (int legacySoftwaresRowId = 0; legacySoftwaresRowId < legacySoftwaresBeforeInsertion.Rows.Count; legacySoftwaresRowId++)
            {
                Fieldset fieldsetSoftware = new Fieldset();

                string softwareID = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<int>(1).ToString();
                string title = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(2);
                string recipients = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(3);
                string shortBlurb = CleanHtml.CleanUpHtml(legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(4), modeCode);
                string info = CleanHtml.CleanUpHtml(legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(5), modeCode);

                title = CleanHtml.ReplaceUnicodeText(title);

                fieldsetSoftware.Alias = "software";
                fieldsetSoftware.Disabled = false;
                fieldsetSoftware.Id = new Guid();
                fieldsetSoftware.Properties = new List<Property>();
                fieldsetSoftware.Properties.Add(new Property("softwareID", softwareID)); // set the file package name
                fieldsetSoftware.Properties.Add(new Property("title", title.Replace("–", "-"))); // set the title
                fieldsetSoftware.Properties.Add(new Property("recipients", recipients.Replace(" ", ""))); // set the recipients email addresses
                fieldsetSoftware.Properties.Add(new Property("shortBlurb", shortBlurb)); // set the short blurb
                fieldsetSoftware.Properties.Add(new Property("information", info)); // set the large text information
                                                                                    // Files
                List<string> filePathSP2List = new List<string>();
                filePathSP2List = ReadFromTextfile(Convert.ToInt32(softwareID));

                //get files from software id folder
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


                        string fileListJson = JsonConvert.SerializeObject(softwareFilesList, Newtonsoft.Json.Formatting.None, jsonSettingsForSoftware);
                        fieldsetSoftware.Properties.Add(new Property("fileDownloads", fileListJson)); // set the large text information
                    }
                    // LOOP END for files
                }

                softwareItem.Fieldsets.Add(fieldsetSoftware);
                // LOOP END
            }

            if (softwareItem != null && softwareItem.Fieldsets != null && softwareItem.Fieldsets.Count > 0)
            {
                filePackageJson = JsonConvert.SerializeObject(softwareItem, Newtonsoft.Json.Formatting.None, jsonSettingsForSoftware);
            }

            // Last, we set the ApiProperty for "software"
            if (false == string.IsNullOrEmpty(filePackageJson))
            {
                softwareProperty = new ApiProperty("software", filePackageJson);
            }
            else
            {
                softwareProperty = new ApiProperty("software", "");
            }

            AddLog(" - Software added: " + legacySoftwaresBeforeInsertion.Rows.Count);

            return softwareProperty;
        }


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
            LOG_FILE_TEXT += line + "\r\n";
        }

    }
}