using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using USDA_ARS.Umbraco.Extensions.Models.Import;

using System.Xml;
using USDA_ARS.LocationsWebApp.DL;
using System.Data;
using Archetype.Models;
using USDA_ARS.LocationsWebApp.Models;
using System.Text.RegularExpressions;

using System.Data.SqlClient;

namespace USDA_ARS.LocationsWebApp
{
    public partial class AddUpdateLocations : System.Web.UI.Page
    {
        protected string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        protected string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        public static string LocationConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        #region single insert buttons-Areas,Cities,Research Units,Labs
        protected void btnImport_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            content.Id = 0; // New page
            content.Name = "Test Page";
            content.ParentId = 1111;
            content.DocType = "Region";
            content.Template = "Region";

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("modeCode", "90-00-00-00")); // Region mode code
            properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=50-00-00-00")); // current URL
            properties.Add(new ApiProperty("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).

            properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,





            content.Properties = properties;

            content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request);

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Umbraco Import Success: " + responseContent.Success + "<br />\r\n";

                        if (false == responseContent.Success)
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "Content Id: " + responseContent.Id + "<br />\r\n";
                        output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                        output.Text += "<br />\r\n";
                    }
                }
            }
        }


        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtIdUpdate.Text); // Update a page
            content.Name = txtName.Text;

            List<ApiProperty> properties = new List<ApiProperty>();

            properties.Add(new ApiProperty("modeCode", "80-00-00-00")); // Region mode code
            properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=80-00-00-00")); // current URL
            properties.Add(new ApiProperty("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).

            content.Properties = properties;

            content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request);

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Umbraco Import Success: " + responseContent.Success + "<br />\r\n";

                        if (false == responseContent.Success)
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "Content Id: " + responseContent.Id + "<br />\r\n";
                        output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                        output.Text += "<br />\r\n";
                    }
                }
            }
        }


        protected void btnGet_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtId.Text); // Load page

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                        if (true == responseContent.Success)
                        {
                            output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                            output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                            output.Text += "<strong>Properties</strong><br />\r\n";

                            foreach (ApiProperty property in responseContent.Properties)
                            {
                                output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                            }
                        }
                        else
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "<br />\r\n";
                    }
                }
            }
        }


        protected void btnGetByModeCode_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            content.Id = 0;
            content.Properties = new List<ApiProperty>();
            content.Properties.Add(new ApiProperty("modeCode", txtModeCode.Text.ToString())); // Load page by property value
            content.Properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                        if (true == responseContent.Success)
                        {
                            output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                            output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                            output.Text += "<strong>Properties</strong><br />\r\n";

                            foreach (ApiProperty property in responseContent.Properties)
                            {
                                output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                            }
                        }
                        else
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "<br />\r\n";
                    }
                }

            }
        }


        protected void btnGetChild_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtParentId.Text); // Load page

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                        if (true == responseContent.Success)
                        {
                            output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                            output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                            output.Text += "<strong>Properties</strong><br />\r\n";

                            foreach (ApiProperty property in responseContent.Properties)
                            {
                                output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                            }

                            if (responseContent.ChildContentList != null)
                            {
                                outputChild.Text = "";

                                foreach (ApiContent childContent in responseContent.ChildContentList)
                                {
                                    outputChild.Text += " - Child Content Umbraco Id: " + childContent.Id + "<br />\r\n";
                                    outputChild.Text += " - Child Content Name: " + childContent.Name + "<br /><br />\r\n";
                                }
                            }
                        }
                        else
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "<br />\r\n";
                    }
                }
            }
        }
        protected void btnAddNewArea_Click(object sender, EventArgs e)
        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;


            string oldId = txtOldId.Text;

            content.Id = 0; // New page
            content.Name = txtParentAreaName.Text;
            content.ParentId = 1111;
            content.DocType = "Region";
            content.Template = "Region";

            List<ApiProperty> properties = new List<ApiProperty>();
            string newModeCodeProperty = txtNewModeCode.Text;
            string oldModeCodeProperty = txtOldModeCode.Text;

            properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
            properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
            properties.Add(new ApiProperty("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
            properties.Add(new ApiProperty("quickLinks", oldId));

            properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,



            // USED FOR ALL ARCHETYPE DATA TYPES
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();


            //===================================================================

            // ADD SOFTWARE
            ApiArchetype softwareItem = new ApiArchetype();

            softwareItem.Fieldsets = new List<Fieldset>();

            // LOOP START
            Fieldset fieldsetSoft = new Fieldset();

            fieldsetSoft.Alias = "software";
            fieldsetSoft.Disabled = false;
            fieldsetSoft.Id = Guid.NewGuid();
            fieldsetSoft.Properties = new List<Property>();
            fieldsetSoft.Properties.Add(new Property("softwareID", "124")); // set the file package name
            fieldsetSoft.Properties.Add(new Property("title", "Cotton Production Model")); // set the file package name
            fieldsetSoft.Properties.Add(new Property("recipients", "john@gmail.com,kiran@gmail.com")); // set the recipients email addresses
            fieldsetSoft.Properties.Add(new Property("shortBlurb", "<p>A new process-based cotton model, CPM, has been developed to...</p>")); // set the short blurb
            fieldsetSoft.Properties.Add(new Property("information", "<p>Long information text here</p>")); // set the large text information

            // Files
            {
                ApiArchetype softwareFilesList = new ApiArchetype();
                softwareFilesList.Fieldsets = new List<Fieldset>();

                // LOOP Through the list of files
                {
                    Fieldset fieldsetFiles = new Fieldset();

                    fieldsetFiles.Alias = "softwareDownloads";
                    fieldsetFiles.Disabled = false;
                    fieldsetFiles.Id = Guid.NewGuid();
                    fieldsetFiles.Properties = new List<Property>();
                    fieldsetFiles.Properties.Add(new Property("file", "/ARSUserFiles/20000000/software/Brio-Insight_en.zip")); // set the file path

                    softwareFilesList.Fieldsets.Add(fieldsetFiles);

                    string fileListJson = JsonConvert.SerializeObject(softwareFilesList, Newtonsoft.Json.Formatting.None, jsonSettings);
                    fieldsetSoft.Properties.Add(new Property("fileDownloads", fileListJson)); // set the large text information
                }
                // LOOP END for files
            }
            //
            softwareItem.Fieldsets.Add(fieldsetSoft);
            // LOOP END


            string filePackageJson = JsonConvert.SerializeObject(softwareItem, Newtonsoft.Json.Formatting.None, jsonSettings);
            properties.Add(new ApiProperty("software", filePackageJson));





            //===================================================================

            // ADD CAROUSEL SLIDES
            ApiArchetype carouselSlide = new ApiArchetype();

            carouselSlide.Fieldsets = new List<Fieldset>();

            // Here is where you would loop through each Carousel Slide Link
            // LOOP START
            string slideFilePath = null; // If a slide links to a file instead of a page, set it here.

            Fieldset fieldsetCar = new Fieldset();

            fieldsetCar.Alias = "slide";
            fieldsetCar.Disabled = false;
            fieldsetCar.Id = Guid.NewGuid();
            fieldsetCar.Properties = new List<Property>();
            fieldsetCar.Properties.Add(new Property("slideName", "ARS Commitment")); // set the slide name
            fieldsetCar.Properties.Add(new Property("slideImage", "/ARSUserFiles/00000000/images/PhotoCarousel/ARS1890-B.png")); // set the slide image path
            fieldsetCar.Properties.Add(new Property("slideText", "<p>Slide Text Here</p>")); // set the slide html text
            fieldsetCar.Properties.Add(new Property("slideAltText", "Slide Alt Text Here")); // set the slide alt text

            // if slide file path is not empty, set it
            if (false == string.IsNullOrEmpty(slideFilePath))
            {
                fieldsetCar.Properties.Add(new Property("slideFile", slideFilePath)); // set the slide file path
                fieldsetCar.Properties.Add(new Property("slideUrl", "")); // set the slide url to empty
            }
            else // Set the URL instead.
            {
                Link linkSlide = new Link("/research/", "/research/", ""); // set the url path
                fieldsetCar.Properties.Add(new Property("slideUrl", "[" + JsonConvert.SerializeObject(linkSlide, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));

                fieldsetCar.Properties.Add(new Property("slideFile", "")); // set the slide alt text
            }
            carouselSlide.Fieldsets.Add(fieldsetCar);

            // LOOP END

            // Last, we set the ApiProperty for "carouselSlide"
            string slideJson = JsonConvert.SerializeObject(carouselSlide, Newtonsoft.Json.Formatting.None, jsonSettings);
            properties.Add(new ApiProperty("carouselSlides", slideJson));



            //===================================================================

            // ADD POPULAR TOPICS
            ApiArchetype popularTopics = new ApiArchetype();

            popularTopics.Fieldsets = new List<Fieldset>();

            // Here is where you would loop through each Popular Topics Link
            // LOOP START
            Fieldset fieldset = new Fieldset();

            fieldset.Alias = "popularTopics";
            fieldset.Disabled = false;
            fieldset.Id = Guid.NewGuid();
            fieldset.Properties = new List<Property>();
            fieldset.Properties.Add(new Property("label", "Bee Health")); // set the label name

            Link link = new Link("http://planthardiness.ars.usda.gov/PHZMWeb/", "http://planthardiness.ars.usda.gov/PHZMWeb/", ""); // set the url path
            fieldset.Properties.Add(new Property("link", "[" + JsonConvert.SerializeObject(link, Newtonsoft.Json.Formatting.None, jsonSettings) + "]"));

            popularTopics.Fieldsets.Add(fieldset);

            // LOOP END


            // Last, we set the ApiProperty for "popularTopics"
            properties.Add(new ApiProperty("popularTopics", JsonConvert.SerializeObject(popularTopics, Newtonsoft.Json.Formatting.None, jsonSettings)));








            //properties.Add(new ApiProperty("modeCode", "90-00-00-00")); // Region mode code
            //properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=50-00-00-00")); // current URL
            // properties.Add(new ApiProperty("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).



            content.Properties = properties;
            content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Post");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                        if (true == responseContent.Success)
                        {
                            output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                            output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                        }
                        else
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "<br />\r\n";
                    }
                }
            }
        }

        protected void btnAddNewCity_Click(object sender, EventArgs e)
        {
            // === ADD NEW CITY ===
            // Set the parent ID. You will need to get the Content ID for the Area the city is under.

            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            string oldId = txtCityOldIdSP.Text;
            string stateCode = txtStateCode.Text.ToUpper();
            string cityName = txtCityName.Text.ToUpper();
            string cityStateConcatenatedString = cityName + "," + stateCode;
            content.Id = 0; // New page
                            // content.Name = "{City Name, State Code}";
                            //content.ParentId = { The Umbraco Content ID for the AREA};
            content.Name = cityStateConcatenatedString;
            content.ParentId = Convert.ToInt32(txtParentAreaId.Text);
            content.DocType = "City";
            content.Template = ""; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();
            string newModeCodeProperty = txtParentAreaModeCode.Text;
            string oldModeCodeProperty = txtOldModeCode.Text;

            properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
            properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
            properties.Add(new ApiProperty("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
            properties.Add(new ApiProperty("state", txtStateCode.Text)); // For example: NY (2 letter state code)
            properties.Add(new ApiProperty("navigationTitle", cityStateConcatenatedString)); // All CAPS - For example: GENEVA, NY

            properties.Add(new ApiProperty("usajobsLocationID", "USAJOBS LOCATION ID HERE")); // USDAJOBS Location ID

            properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

            //properties.Add(new ApiProperty("modeCode", "80-10-00-00")); // Region mode code
            //properties.Add(new ApiProperty("oldUrl", "")); // Leave blank since there is no city page on the website.
            //properties.Add(new ApiProperty("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
            //properties.Add(new ApiProperty("state", "{State Code}")); // For example: NY (2 letter state code)
            //properties.Add(new ApiProperty("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Post");

            if (responseBack.ContentList != null)
            {
                foreach (ApiContent responseContent in responseBack.ContentList)
                {
                    output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                    if (true == responseContent.Success)
                    {
                        output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                        output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                    }
                    else
                    {
                        output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                    }

                    output.Text += "<br />\r\n";
                }
            }

        }

        protected void btnAddNewRC_Click(object sender, EventArgs e)
        {
            // === ADD NEW LAB ===
            // Set the parent ID. You will need to get the Content ID for the Area the city is under.

            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;

            string oldId = txtCityOldIdSP.Text;
            string stateCode = txtStateCode.Text.ToUpper();
            string cityName = txtCityName.Text.ToUpper();
            string cityStateConcatenatedString = cityName + "," + stateCode;
            content.Id = 0; // New page
                            // content.Name = "{City Name, State Code}";
                            //content.ParentId = { The Umbraco Content ID for the AREA};
            content.Name = cityStateConcatenatedString;
            content.ParentId = Convert.ToInt32(txtParentAreaId.Text);
            content.DocType = "ResearchUnit";
            content.Template = "ResearchUnit"; // Leave blank

            List<ApiProperty> properties = new List<ApiProperty>();
            string newModeCodeProperty = txtParentAreaModeCode.Text;
            string oldModeCodeProperty = txtOldModeCode.Text;

            properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
            properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
            properties.Add(new ApiProperty("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).

            properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

            //properties.Add(new ApiProperty("state", txtStateCode.Text)); // For example: NY (2 letter state code)
            //properties.Add(new ApiProperty("navigationTitle", cityStateConcatenatedString)); // All CAPS - For example: GENEVA, NY

            //properties.Add(new ApiProperty("modeCode", "80-10-00-00")); // Region mode code
            //properties.Add(new ApiProperty("oldUrl", "")); // Leave blank since there is no city page on the website.
            //properties.Add(new ApiProperty("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
            //properties.Add(new ApiProperty("state", "{State Code}")); // For example: NY (2 letter state code)
            //properties.Add(new ApiProperty("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            request.ContentList = new List<ApiContent>();
            request.ContentList.Add(content);

            ApiResponse responseBack = PostData(request, "Post");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null)
                {
                    foreach (ApiContent responseContent in responseBack.ContentList)
                    {
                        output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                        if (true == responseContent.Success)
                        {
                            output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                            output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                        }
                        else
                        {
                            output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                        }

                        output.Text += "<br />\r\n";
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Posts a JSON Content Object to the Umbraco API
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private ApiResponse PostData(ApiRequest request, string endPoint = "Post")
        {
            ApiResponse response = null;
            string apiUrl = API_URL;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl + endPoint));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            http.Timeout = 216000;
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




        protected void btnAddMultipleAreas_Click(object sender, EventArgs e)
        {
            // Clean output message
            output.Text = "";
            //1.connection string
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //2.all locations -=retrieval from old db and inserting into new db using umbraco

            ////2.1 all areas---retrieval and insertion

            System.Data.DataTable newAreasAfterInsertion = new System.Data.DataTable();
            newAreasAfterInsertion = AddAllAreas();




            ////2.2 all cities---retrieval and insertion
            System.Data.DataTable newCitiesAfterInsertion = new System.Data.DataTable();
             newCitiesAfterInsertion = AddAllCities(newAreasAfterInsertion);

            ////2.3 all RCs---retrieval and insertion
            System.Data.DataTable newResearchUnitsAfterInsertion = new System.Data.DataTable();
             newResearchUnitsAfterInsertion = AddAllResearchUnits(newCitiesAfterInsertion);

            ////2.4 all Labs---retrieval and insertion
            System.Data.DataTable newLabsAfterInsertion = new System.Data.DataTable();
             newLabsAfterInsertion = AddAllLabs(newResearchUnitsAfterInsertion);

        }
        protected DataTable AddAllAreas()

        {

            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            DataTable newAreasAfterInsertion = new DataTable();
            newAreasAfterInsertion.Columns.Add("UmbracoId");
            newAreasAfterInsertion.Columns.Add("Name");
            newAreasAfterInsertion.Columns.Add("ModeCode");
            newAreasAfterInsertion.Columns.Add("QuickLinks");
            newAreasAfterInsertion.Columns.Add("webtrendsProfileID");
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
                string oldModeCodeProperty = completeModeCode;

                properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                properties.Add(new ApiProperty("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
                properties.Add(new ApiProperty("quickLinks", quickLinks));
                properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));

                properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

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

                ApiResponse responseBack = PostData(request, "Post");

                if (responseBack != null)
                {
                    output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                    output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                    output.Text += "<br />\r\n";

                    if (responseBack.ContentList != null)
                    {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                            output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                            if (true == responseContent.Success)
                            {
                                output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                                output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                                newAreasAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value, responseContent.Properties[3].Value, responseContent.Properties[4].Value);
                            }
                            else
                            {
                                output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                            }

                            output.Text += "<br />\r\n";
                        }
                    }
                }
            }
            for (int rowId = 0; rowId < newAreasAfterInsertion.Rows.Count; rowId++)
            {

                DL.PersonSite.AddPeopleSites(newAreasAfterInsertion.Rows[rowId].Field<string>(2));
            }

            return newAreasAfterInsertion;

        }

        protected DataTable AddAllCities(DataTable newAreasAfterInsertion)

        {

            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            DataTable newCitiesAfterInsertion = new DataTable();

            newCitiesAfterInsertion.Columns.Add("UmbracoId");
            newCitiesAfterInsertion.Columns.Add("Name");
            newCitiesAfterInsertion.Columns.Add("ModeCode");
            newCitiesAfterInsertion.Columns.Add("StateCode");


            ////3.all cities--retrieval of new areas umbraco ids,retrieval of old cities based on those modecodes,insertion of new cities

            System.Data.DataTable legacyCitiesBeforeInsertion = new System.Data.DataTable();


            for (int i = 0; i < newAreasAfterInsertion.Rows.Count; i++)
            {

                int parentAreaUmbracoId = Convert.ToInt32(newAreasAfterInsertion.Rows[i].Field<string>(0));
                string parentAreaModeCode = newAreasAfterInsertion.Rows[i].Field<string>(2);
                if (parentAreaModeCode.Length < 11)
                    parentAreaModeCode = "0" + parentAreaModeCode;
                legacyCitiesBeforeInsertion = AddRetrieveLocationsDL.GetAllCities(Convert.ToInt32(parentAreaModeCode.Substring(0, 2)));
                for (int j = 0; j < legacyCitiesBeforeInsertion.Rows.Count; j++)
                {

                    string cityNameWithStateName = legacyCitiesBeforeInsertion.Rows[j].Field<string>(2);
                    string stateCode = legacyCitiesBeforeInsertion.Rows[j].Field<string>(3);
                    string cityNameWithStateCode = cityNameWithStateName.Split(',')[0] + '-' + stateCode;
                    string cityNameWithStateCodeWithComma = cityNameWithStateName.Split(',')[0].ToUpper() + ", " + stateCode;


                    request.ApiKey = API_KEY;
                    content.Id = 0; // New page
                                    // content.Name = "{City Name, State Code}";
                                    //content.ParentId = { The Umbraco Content ID for the AREA};
                    content.Name = cityNameWithStateName;
                    content.ParentId = parentAreaUmbracoId;
                    content.DocType = "City";
                    content.Template = ""; // Leave blank

                    List<ApiProperty> properties = new List<ApiProperty>();
                    string newModeCodeProperty = legacyCitiesBeforeInsertion.Rows[j].Field<string>(1);
                    string oldModeCodeProperty = legacyCitiesBeforeInsertion.Rows[j].Field<string>(1);

                    properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                    properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                    properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                                                                  //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                    properties.Add(new ApiProperty("state", stateCode)); // For example: NY (2 letter state code)
                    properties.Add(new ApiProperty("navigationTitle", cityNameWithStateCodeWithComma)); // All CAPS - For example: GENEVA, NY
                    properties.Add(new ApiProperty("umbracoUrlName", cityNameWithStateCode));

                    properties.Add(new ApiProperty("usajobsLocationID", "USAJOBS LOCATION ID HERE")); // USDAJOBS Location ID

                    properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

                    //properties.Add(new ApiProperty("modeCode", "80-10-00-00")); // Region mode code
                    //properties.Add(new ApiProperty("oldUrl", "")); // Leave blank since there is no city page on the website.
                    //properties.Add(new ApiProperty("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
                    //properties.Add(new ApiProperty("state", "{State Code}")); // For example: NY (2 letter state code)
                    //properties.Add(new ApiProperty("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

                    content.Properties = properties;

                    content.Save = 2; // 1=Saved, 2=Save And Publish

                    request.ContentList = new List<ApiContent>();
                    request.ContentList.Add(content);

                    ApiResponse responseBack = PostData(request, "Post");

                    if (responseBack.ContentList != null)
                    {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                            output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                            if (true == responseContent.Success)
                            {
                                output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                                output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                                newCitiesAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value, responseContent.Properties[3].Value);
                            }
                            else
                            {
                                output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                            }

                            output.Text += "<br />\r\n";
                        }
                    }


                }
            }


            return newCitiesAfterInsertion;

        }

        protected DataTable AddAllResearchUnits(DataTable newCitiesAfterInsertion)

        {



            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            DataTable newResearchUnitsAfterInsertion = new DataTable();

            newResearchUnitsAfterInsertion.Columns.Add("UmbracoId");
            newResearchUnitsAfterInsertion.Columns.Add("Name");
            newResearchUnitsAfterInsertion.Columns.Add("ModeCode");
            newResearchUnitsAfterInsertion.Columns.Add("QuickLinks");
            newResearchUnitsAfterInsertion.Columns.Add("webtrendsProfileID");

            ////3.all cities--retrieval of new areas umbraco ids,retrieval of old cities based on those modecodes,insertion of new cities

            System.Data.DataTable legacyResearchUnitsBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyQuickLinksBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyWebTrendsBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyCarouselSlidesBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacySoftwaresBeforeInsertion = new System.Data.DataTable();
            for (int i = 0; i < newCitiesAfterInsertion.Rows.Count; i++)
            {

                int parentCityUmbracoId = Convert.ToInt32(newCitiesAfterInsertion.Rows[i].Field<string>(0));
                string parentLocationModeCode = newCitiesAfterInsertion.Rows[i].Field<string>(2);

                if (parentLocationModeCode.Length < 11)
                    parentLocationModeCode = "0" + parentLocationModeCode;
                legacyResearchUnitsBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnits(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)));

                legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnitsQuickLinks(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)));
                legacyResearchUnitsBeforeInsertion = CompareTwoDataTables(legacyResearchUnitsBeforeInsertion, legacyQuickLinksBeforeInsertion);
                legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnitsWebTrends(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)));
                legacyResearchUnitsBeforeInsertion = CompareTwoDataTables(legacyResearchUnitsBeforeInsertion, legacyWebTrendsBeforeInsertion);
                for (int j = 0; j < legacyResearchUnitsBeforeInsertion.Rows.Count; j++)
                {

                    string rCName = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(3);
                    string quickLinks = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(4);
                    string webtrendsProfileID = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(5);
                    request.ApiKey = API_KEY;
                    content.Id = 0; // New page
                                    // content.Name = "{City Name, State Code}";
                                    //content.ParentId = { The Umbraco Content ID for the AREA};
                    content.Name = rCName;
                    content.ParentId = parentCityUmbracoId;
                    content.DocType = "ResearchUnit";
                    content.Template = "ResearchUnit"; // Leave blank

                    List<ApiProperty> properties = new List<ApiProperty>();
                    string newModeCodeProperty = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(2);
                    string oldModeCodeProperty = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(2);

                    properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                    properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                    properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                    properties.Add(new ApiProperty("quickLinks", quickLinks));                                  //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                    properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));

                    properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

                    legacyCarouselSlidesBeforeInsertion = AddRetrieveLocationsDL.GetAllCarouselSlidesBasedOnModeCode(newModeCodeProperty);
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
                        string slideImageSP2 = "/ARSUserFiles/" + "" + newModeCodeProperty.Replace("-", "") + "/images/PhotoCarousel/" + slideImage;
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
                    legacySoftwaresBeforeInsertion = AddRetrieveLocationsDL.GetAllSoftwaresBasedOnModeCode(newModeCodeProperty);



                    var jsonSettingsForSoftware = new JsonSerializerSettings();
                    jsonSettingsForSoftware.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                    // USED FOR ALL ARCHETYPE DATA TYPES


                    // ADD software
                    ApiArchetype softwareItem = new ApiArchetype();

                    softwareItem.Fieldsets = new List<Fieldset>();

                    string filePackageJson;


                    for (int legacySoftwaresRowId = 0; legacySoftwaresRowId < legacySoftwaresBeforeInsertion.Rows.Count; legacySoftwaresRowId++)
                    {

                        Fieldset fieldsetSoftware = new Fieldset();
                        // Here is where you would loop through each Carousel Slide Link
                        // LOOP START
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
                        fieldsetSoftware.Properties.Add(new Property("info", info)); // set the large text information
                                                                                     // Files
                        string filePathSP2 = "/ARSUserFiles/" + "" + newModeCodeProperty.Replace("-", "") + "/software/Brio-Insight_en.zip";
                        {
                            ApiArchetype softwareFilesList = new ApiArchetype();

                            softwareFilesList.Fieldsets = new List<Fieldset>();
                            //get files from software id folder

                            // LOOP Through the list of files
                            {
                                Fieldset fieldsetFiles = new Fieldset();

                                fieldsetFiles.Alias = "softwareDownloads";
                                fieldsetFiles.Disabled = false;
                                fieldsetFiles.Id = Guid.NewGuid();
                                fieldsetFiles.Properties = new List<Property>();
                                fieldsetFiles.Properties.Add(new Property("file", filePathSP2)); // set the file path

                                softwareFilesList.Fieldsets.Add(fieldsetFiles);

                                string fileListJson = JsonConvert.SerializeObject(softwareFilesList, Newtonsoft.Json.Formatting.None, jsonSettings);
                                fieldsetSoftware.Properties.Add(new Property("fileDownloads", fileListJson)); // set the large text information
                            }
                            // LOOP END for files
                        }

                        softwareItem.Fieldsets.Add(fieldsetSoftware);
                        // Last, we set the ApiProperty for "carouselSlide"
                        filePackageJson = JsonConvert.SerializeObject(softwareItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                        properties.Add(new ApiProperty("software", filePackageJson));
                        // LOOP END
                    }







                    content.Properties = properties;

                    content.Save = 2; // 1=Saved, 2=Save And Publish

                    request.ContentList = new List<ApiContent>();
                    request.ContentList.Add(content);

                    ApiResponse responseBack = PostData(request, "Post");

                    if (responseBack.ContentList != null)
                    {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                            output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                            if (true == responseContent.Success)
                            {
                                output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                                output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                                newResearchUnitsAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value, responseContent.Properties[3].Value, responseContent.Properties[4].Value);
                            }
                            else
                            {
                                output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                            }

                            output.Text += "<br />\r\n";
                        }
                    }


                }
            }

            for (int rowId = 0; rowId < newResearchUnitsAfterInsertion.Rows.Count; rowId++)
            {

                DL.PersonSite.AddPeopleSites(newResearchUnitsAfterInsertion.Rows[rowId].Field<string>(2));
            }

            return newResearchUnitsAfterInsertion;
        }
        protected DataTable AddAllLabs(DataTable newResearchUnitsAfterInsertion)

        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            DataTable newLabsAfterInsertion = new DataTable();

            newLabsAfterInsertion.Columns.Add("UmbracoId");
            newLabsAfterInsertion.Columns.Add("Name");
            newLabsAfterInsertion.Columns.Add("ModeCode");
            newLabsAfterInsertion.Columns.Add("QuickLinks");
            newLabsAfterInsertion.Columns.Add("webtrendsProfileID");

            ////3.all cities--retrieval of new areas umbraco ids,retrieval of old cities based on those modecodes,insertion of new cities

            System.Data.DataTable legacyLabsBeforeInsertion = new System.Data.DataTable();



            System.Data.DataTable legacyQuickLinksBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyWebTrendsBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacyCarouselSlidesBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable legacySoftwaresBeforeInsertion = new System.Data.DataTable();
            for (int i = 0; i < newResearchUnitsAfterInsertion.Rows.Count; i++)
            {

                int parentResearchUnitUmbracoId = Convert.ToInt32(newResearchUnitsAfterInsertion.Rows[i].Field<string>(0));
                string parentLocationModeCode = newResearchUnitsAfterInsertion.Rows[i].Field<string>(2);
                if (parentLocationModeCode.Length < 11)
                    parentLocationModeCode = "0" + parentLocationModeCode;
                legacyLabsBeforeInsertion = AddRetrieveLocationsDL.GetAllLabs(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)), Convert.ToInt32(parentLocationModeCode.Substring(6, 2)));
                legacyQuickLinksBeforeInsertion = AddRetrieveLocationsDL.GetAllLabsQuickLinks(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)), Convert.ToInt32(parentLocationModeCode.Substring(6, 2)));
                legacyLabsBeforeInsertion = CompareTwoDataTables(legacyLabsBeforeInsertion, legacyQuickLinksBeforeInsertion);
                legacyWebTrendsBeforeInsertion = AddRetrieveLocationsDL.GetAllLabsWebTrendsProfileIDs(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)), Convert.ToInt32(parentLocationModeCode.Substring(6, 2))); ;
                legacyLabsBeforeInsertion = CompareTwoDataTables(legacyLabsBeforeInsertion, legacyWebTrendsBeforeInsertion);

                for (int j = 0; j < legacyLabsBeforeInsertion.Rows.Count; j++)
                {

                    string labName = legacyLabsBeforeInsertion.Rows[j].Field<string>(4);
                    string quickLinks = legacyLabsBeforeInsertion.Rows[j].Field<string>(5);
                    string webtrendsProfileID = legacyLabsBeforeInsertion.Rows[j].Field<string>(6);
                    request.ApiKey = API_KEY;
                    content.Id = 0; // New page
                                    // content.Name = "{City Name, State Code}";
                                    //content.ParentId = { The Umbraco Content ID for the AREA};
                    content.Name = labName;
                    content.ParentId = parentResearchUnitUmbracoId;
                    content.DocType = "ResearchUnit";
                    content.Template = "ResearchUnit"; // Leave blank

                    List<ApiProperty> properties = new List<ApiProperty>();
                    string newModeCodeProperty = legacyLabsBeforeInsertion.Rows[j].Field<string>(3);
                    string oldModeCodeProperty = legacyLabsBeforeInsertion.Rows[j].Field<string>(3);

                    properties.Add(new ApiProperty("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                    properties.Add(new ApiProperty("oldUrl", "/PandP/locations/cityPeopleList.cfm?modeCode=" + newModeCodeProperty + "")); // current URL               
                    properties.Add(new ApiProperty("oldId", "")); // sitepublisher ID (So we can reference it later if needed).
                    properties.Add(new ApiProperty("quickLinks", quickLinks));                                              //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                    properties.Add(new ApiProperty("webtrendsProfileID", webtrendsProfileID));

                    properties.Add(new ApiProperty("oldModeCode", "OLD MODE CODES HERE")); // Separate old modes codes by a comma ,

                    legacyCarouselSlidesBeforeInsertion = AddRetrieveLocationsDL.GetAllCarouselSlidesBasedOnModeCode(newModeCodeProperty);
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
                        string slideImageSP2 = "/ARSUserFiles/" + "" + newModeCodeProperty.Replace("-", "") + "/images/PhotoCarousel/" + slideImage;
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
                    legacySoftwaresBeforeInsertion = AddRetrieveLocationsDL.GetAllSoftwaresBasedOnModeCode(newModeCodeProperty);



                    var jsonSettingsForSoftware = new JsonSerializerSettings();
                    jsonSettingsForSoftware.ContractResolver = new LowercaseJsonSerializer.LowercaseContractResolver();

                    // USED FOR ALL ARCHETYPE DATA TYPES


                    // ADD software
                    ApiArchetype softwareItem = new ApiArchetype();

                    softwareItem.Fieldsets = new List<Fieldset>();

                    string filePackageJson;


                    for (int legacySoftwaresRowId = 0; legacySoftwaresRowId < legacySoftwaresBeforeInsertion.Rows.Count; legacySoftwaresRowId++)
                    {

                        Fieldset fieldsetSoftware = new Fieldset();
                        // Here is where you would loop through each Carousel Slide Link
                        // LOOP START
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
                        fieldsetSoftware.Properties.Add(new Property("info", info)); // set the large text information
                        List<string> filePathSP2List = new List<string>();
                        // Files
                        string filePathSP2 = "/ARSUserFiles/" + "" + newModeCodeProperty.Replace("-", "") + "/software/Brio-Insight_en.zip";
                        {
                            ApiArchetype softwareFilesList = new ApiArchetype();

                            softwareFilesList.Fieldsets = new List<Fieldset>();
                            //get files from software id folder

                            // LOOP Through the list of files
                            {
                                Fieldset fieldsetFiles = new Fieldset();

                                fieldsetFiles.Alias = "softwareDownloads";
                                fieldsetFiles.Disabled = false;
                                fieldsetFiles.Id = Guid.NewGuid();
                                fieldsetFiles.Properties = new List<Property>();
                                fieldsetFiles.Properties.Add(new Property("file", filePathSP2)); // set the file path

                                softwareFilesList.Fieldsets.Add(fieldsetFiles);

                                string fileListJson = JsonConvert.SerializeObject(softwareFilesList, Newtonsoft.Json.Formatting.None, jsonSettings);
                                fieldsetSoftware.Properties.Add(new Property("fileDownloads", fileListJson)); // set the large text information
                            }
                            // LOOP END for files
                        }

                        softwareItem.Fieldsets.Add(fieldsetSoftware);
                        // Last, we set the ApiProperty for "carouselSlide"
                        filePackageJson = JsonConvert.SerializeObject(softwareItem, Newtonsoft.Json.Formatting.None, jsonSettings);
                        properties.Add(new ApiProperty("software", filePackageJson));
                        // LOOP END
                    }








                    content.Properties = properties;

                    content.Save = 2; // 1=Saved, 2=Save And Publish

                    request.ContentList = new List<ApiContent>();
                    request.ContentList.Add(content);

                    ApiResponse responseBack = PostData(request, "Post");

                    if (responseBack.ContentList != null)
                    {
                        foreach (ApiContent responseContent in responseBack.ContentList)
                        {
                            output.Text += "Get Content Success: " + responseContent.Success + "<br />\r\n";

                            if (true == responseContent.Success)
                            {
                                output.Text += "Content Umbraco Id: " + responseContent.Id + "<br />\r\n";
                                output.Text += "Content Name: " + responseContent.Name + "<br />\r\n";
                                newLabsAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value, responseContent.Properties[3].Value, responseContent.Properties[4].Value);
                            }
                            else
                            {
                                output.Text += "Fail Message: " + responseContent.Message + "<br />\r\n";
                            }

                            output.Text += "<br />\r\n";
                        }
                    }


                }
            }

            for (int rowId = 0; rowId < newLabsAfterInsertion.Rows.Count; rowId++)
            {

                DL.PersonSite.AddPeopleSites(newLabsAfterInsertion.Rows[rowId].Field<string>(2));
            }

            return newLabsAfterInsertion;
        }
        // protected DataTable AddAllQuickLinks(DataTable newLocations) { }
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
            string[] lines = File.ReadAllLines("C:\\get_files.txt");

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

        protected void getSoftwareFoldersFiles_Click(object sender, EventArgs e)
        {
            int SoftwareId = Convert.ToInt32(txtgetSoftwareFoldersFiles.Text);
            List<string> myCollection = new List<string>();
            myCollection = ReadFromTextfile(SoftwareId);
        }

        protected void btnAddMultipleNationalPrograms_Click(object sender, EventArgs e)
        {
            NationalPrograms.ImportNationPrograms();
        }

        protected void btnRandomIds_Click(object sender, EventArgs e)
        {
            string randomId = txtRandomIds.Text;
            DataTable legacyRandomDocuments = new DataTable();
            legacyRandomDocuments=GetAllRandomDocuments(randomId);
        }
        public static DataTable GetAllRandomDocuments(string randomId)
        {

            DataTable legacyRandomDocuments = new DataTable();
            DataTable legacyRandomDocumentsDocPagesEncrypted = new DataTable();
            DataTable legacyRandomDocPagesDecrypted = new DataTable();


            legacyRandomDocumentsDocPagesEncrypted = GetAllRandomDocumentsDocPagesEncrypted(randomId);

            for (int legacyRandomDocumentsDocPagesEncryptedRowId = 0; legacyRandomDocumentsDocPagesEncryptedRowId < legacyRandomDocumentsDocPagesEncrypted.Rows.Count; legacyRandomDocumentsDocPagesEncryptedRowId++)
            {

                legacyRandomDocPagesDecrypted = GetAllRandomDocPagesDecrypted(randomId);
            }
            return legacyRandomDocuments;

        }
        public static DataTable GetAllRandomDocumentsDocPagesEncrypted(string randomId)
        {
            // YOU WILL NEED TO GET THE DOCUMENTS BY THE NP CODE

            //SELECT title, rtrim(doctype) as doctype, docid
            //FROM documents d
            //WHERE d.originsite_type = 'program'
            //AND d.originsite_id = @npCode
            //AND d.published = 'p'
            //AND d.SPSysEndTime is null
            //ORDER BY rtrim(doctype), title, docid

            // THE ABOVE SQL STATEMENT WILL GET YOU THE DOC TITLE, DOC TYPE, AND DOC ID






            Locations locationsResponse = new Locations();
            string sql = "[uspgetAllDocumentsBasedOnRandomDocIds]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlComm.Parameters.AddWithValue("@RandomDocId", randomId);

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                //foreach (DataRow dr in dt.Rows)
                //{
                //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
                //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



                //}



            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            //return locationsResponse;
            return dt;
        }

        public static DataTable GetAllRandomDocPagesDecrypted(string docPageEncrypted)
        {





            Locations locationsResponse = new Locations();
            string sql = "[uspGetAllNPDocPagesDecrypted]";
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(LocationConnectionString);

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlComm = new SqlCommand(sql, conn);


                da.SelectCommand = sqlComm;
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlComm.Parameters.AddWithValue("@DocPageEncrypted", docPageEncrypted);

                DataSet ds = new DataSet();
                da.Fill(ds, "Locations");

                dt = ds.Tables["Locations"];
                //foreach (DataRow dr in dt.Rows)
                //{
                //    locationsResponse.LocationModeCode = dr["MODECODE_1"].ToString();
                //    locationsResponse.LocationName = dr["MODECODE_1_DESC"].ToString();



                //}



            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            //return locationsResponse;
            return dt;
        }
    }



}
