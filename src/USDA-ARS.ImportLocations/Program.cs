using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        static string LOG_FILE_TEXT = "";

        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        static List<ModeCodeLookup> MODE_CODE_LIST = null;

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
            List<Area> areaList = new List<Area>();

            areaList = AddAllAreas();
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

            System.Data.DataTable legacyAreasBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyQuickLinksBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyWebTrendsBeforeInsertion = new System.Data.DataTable();

            legacyAreasBeforeInsertion = AddRetrieveLocationsDL.GetAllAreas();
            legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllAreasQuickLinks();
            legacyAreasBeforeInsertion = CompareTwoDataTables(legacyAreasBeforeInsertion, legacyQuickLinksBeforeInsertion);
            legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllAreasWebTrendsProfileIDs();
            legacyAreasBeforeInsertion = CompareTwoDataTables(legacyAreasBeforeInsertion, legacyWebTrendsBeforeInsertion);

            System.Data.DataTable legacyCarouselSlidesBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacySoftwaresBeforeInsertion = new System.Data.DataTable();

            for (int i = 0; i < legacyAreasBeforeInsertion.Rows.Count; i++)
            {
                string completeModeCode = legacyAreasBeforeInsertion.Rows[i].Field<string>(0);
                string areaName = legacyAreasBeforeInsertion.Rows[i].Field<string>(1);
                string quickLinks = legacyAreasBeforeInsertion.Rows[i].Field<string>(2);
                string webtrendsProfileID = legacyAreasBeforeInsertion.Rows[i].Field<string>(3);
                if (completeModeCode.Length < 11)
                    completeModeCode = "0" + completeModeCode;

                string oldId = "";

                content.Id = 0; // New page
                content.Name = areaName;
                content.ParentId = 1111;
                content.DocType = "Region";
                content.Template = "Region";

                List<ApiProperty> properties = new List<ApiProperty>();
                string newModeCodeProperty = completeModeCode;
                string oldModeCodeProperty = "";

                properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                properties.Add(new ApiProperty("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
                properties.Add(new ApiProperty("quickLinks", quickLinks));
                properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));

                System.Data.DataTable legacyOldModeCodes = new System.Data.DataTable();
                legacyOldModeCodes = AddRetrieveLocationsDL.GetAllOldModeCodesBasedOnNewModeCodes(newModeCodeProperty);
                if (legacyOldModeCodes.Rows.Count > 0)
                {
                    oldModeCodeProperty = legacyOldModeCodes.Rows[0].Field<string>(1);
                }
                properties.Add(new ApiProperty("oldModeCodes", oldModeCodeProperty)); // Separate old modes codes by a comma ,

                legacyCarouselSlidesBeforeInsertion = AddRetrieveLocationsDL.GetAllCarouselSlidesBasedOnModeCode(completeModeCode);
                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                // USED FOR ALL ARCHETYPE DATA TYPES


                // ADD CAROUSEL
                ApiArchetype carouselSlide = new ApiArchetype();

                carouselSlide.Fieldsets = new List<Fieldset>();
                string slideJson;


                for (int legacyCarouselSlidesRowId = 0; legacyCarouselSlidesRowId < legacyCarouselSlidesBeforeInsertion.Rows.Count; legacyCarouselSlidesRowId++)
                {

                    Fieldset fieldsetCar = new Fieldset();
                    // Here is where you would loop through each Carousel Slide Link
                    // LOOP START
                    string slideName = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(5);
                    string slideText = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(4);
                    string slideAltText = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(3);
                    string slideURL = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(2);
                    string slideImage = legacyCarouselSlidesBeforeInsertion.Rows[legacyCarouselSlidesRowId].Field<string>(1);
                    string slideImageSP2 = "/ARSUserFiles/" + "" + completeModeCode.Replace("-", "") + "/images/PhotoCarousel/" + slideImage;
                    string slideFilePath = null;
                    if (slideURL.Contains("/ARSUserFiles/"))
                    {
                        slideFilePath = slideImageSP2; // If a slide links to a file instead of a page, set it here.
                    }




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
                            Link linkSlide = new Link("/research/", "/research/", slideURL); // set the url path
                            fieldsetCar.Properties.Add(new Property("link", "[" + JsonConvert.SerializeObject(linkSlide, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));
                        }
                        else { fieldsetCar.Properties.Add(new Property("slideUrl", "")); }
                        fieldsetCar.Properties.Add(new Property("slideFile", "")); // set the slide alt text
                    }
                    carouselSlide.Fieldsets.Add(fieldsetCar);
                    // Last, we set the ApiProperty for "carouselSlide"
                    slideJson = JsonConvert.SerializeObject(carouselSlide, Newtonsoft.Json.Formatting.None, jsonSettings);
                    properties.Add(new ApiProperty("carouselSlides", slideJson));
                    // LOOP END
                }




                ////softwares
                legacySoftwaresBeforeInsertion = AddRetrieveLocationsDL.GetAllSoftwaresBasedOnModeCode(completeModeCode);


                // USED FOR ALL ARCHETYPE DATA TYPES
                var jsonSettingsForSoftware = new JsonSerializerSettings();
                jsonSettingsForSoftware.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();




                // ADD software
                ApiArchetype softwareItem = new ApiArchetype();
                softwareItem.Fieldsets = new List<Fieldset>();
                string filePackageJson;

                // Here is where you would loop through each Carousel Slide Link
                // LOOP START
                for (int legacySoftwaresRowId = 0; legacySoftwaresRowId < legacySoftwaresBeforeInsertion.Rows.Count; legacySoftwaresRowId++)
                {
                    Fieldset fieldsetSoftware = new Fieldset();


                    string softwareID = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<int>(1).ToString();
                    string title = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(2);
                    string recipients = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(3);
                    string shortBlurb = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(4);
                    string info = legacySoftwaresBeforeInsertion.Rows[legacySoftwaresRowId].Field<string>(5);




                    fieldsetSoftware.Alias = "software";
                    fieldsetSoftware.Disabled = false;
                    fieldsetSoftware.Id = new Guid();
                    fieldsetSoftware.Properties = new List<Property>();
                    fieldsetSoftware.Properties.Add(new Property("softwareID", softwareID)); // set the file package name
                    fieldsetSoftware.Properties.Add(new Property("title", title)); // set the title
                    fieldsetSoftware.Properties.Add(new Property("recipients", info)); // set the recipients email addresses
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
                                string filePathSP2 = "/ARSUserFiles/" + "" + completeModeCode.Replace("-", "") + "/software/" + filePathSP2List[filePathSP2ListRowId];
                                fieldsetFiles.Properties.Add(new Property("file", filePathSP2)); // set the file path
                                softwareFilesList.Fieldsets.Add(fieldsetFiles);
                            }


                            string fileListJson = JsonConvert.SerializeObject(softwareFilesList, Newtonsoft.Json.Formatting.None, jsonSettings);
                            fieldsetSoftware.Properties.Add(new Property("fileDownloads", fileListJson)); // set the large text information
                        }
                        // LOOP END for files
                    }

                    softwareItem.Fieldsets.Add(fieldsetSoftware);
                    // LOOP END



                }
                filePackageJson = JsonConvert.SerializeObject(softwareItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                properties.Add(new ApiProperty("software", filePackageJson));
                content.Properties = properties;
                content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

                request.ContentList = new List<ApiContent>();
                request.ContentList.Add(content);

                ApiResponse responseBack = ApiCalls.PostData(request, "Post");

                if (responseBack != null)
                {
                    AddLog("Success: " + responseBack.Success);
                    AddLog("Message: " + responseBack.Message);
                    AddLog("");

                    if (responseBack.ContentList != null)
                    {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                            AddLog("Get Content Success: " + responseContent.Success);

                            if (true == responseContent.Success)
                            {
                                AddLog("Content Umbraco Id: " + responseContent.Id);
                                AddLog("Content Name: " + responseContent.Name);

                                areaList.Add(new Area() { UmbracoId = responseContent.Id, Name = responseContent.Name, ModeCode = responseContent.Properties[0].Value.ToString(), QuickLinks = responseContent.Properties[3].Value.ToString(), WebtrendsProfileID = responseContent.Properties[4].Value.ToString() });
                            }
                            else
                            {
                                AddLog("Fail Message: " + responseContent.Message);
                            }

                            AddLog("");
                        }
                    }
                }
            }
            ////for (int rowId = 0; rowId < newAreasAfterInsertion.Rows.Count; rowId++)
            ////{

            ////    DL.PersonSite.AddPeopleSites(newAreasAfterInsertion.Rows[rowId].Field<string>(2));
            ////}

            return areaList;

        }


        public static DataTable CompareTwoDataTables(DataTable dataTable1, DataTable dataTable2)
        {
            DataTable dataTable3 = new DataTable();
            dataTable1.PrimaryKey = new DataColumn[] { dataTable1.Columns["Mode Code"] };
            dataTable2.PrimaryKey = new DataColumn[] { dataTable2.Columns["Mode Code"] };
            dataTable3 = dataTable1.Copy();
            dataTable3.Merge(dataTable2, false, MissingSchemaAction.Add);
            dataTable3.AcceptChanges();

            return dataTable3;
        }


        public static List<string> ReadFromTextfile(int softwareId)
        {
            string current_path = "getFileNamesForSoftwareIdFolders.txt";

            string[] lines = System.IO.File.ReadAllLines(current_path);

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


        static void AddLog(string line)
        {
            Debug.WriteLine(line);
            Console.WriteLine(line);
        }

    }
}
