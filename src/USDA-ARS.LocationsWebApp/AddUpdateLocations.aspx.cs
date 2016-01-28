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

namespace USDA_ARS.LocationsWebApp
{
    public partial class AddUpdateLocations : System.Web.UI.Page
    {
        protected string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        protected string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        protected void Page_Load(object sender, EventArgs e)
        {

        }

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

        protected void btnAddMultipleAreas_Click(object sender, EventArgs e)
        {
            // Clean output message
            output.Text = "";
            //1.connection string
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //2.all locations -=retrieval from old db and inserting into new db using umbraco
                    
                    ////2.1 all areas---retrieval and insertion
            System.Data.DataTable legacyAreasBeforeInsertion = new System.Data.DataTable();
            System.Data.DataTable newAreasAfterInsertion = new System.Data.DataTable();
            legacyAreasBeforeInsertion = AddRetrieveLocationsDL.GetAllAreas();                    
            newAreasAfterInsertion = AddAllAreas(legacyAreasBeforeInsertion);

                    ////2.2 all cities---retrieval and insertion
            System.Data.DataTable newCitiesAfterInsertion = new System.Data.DataTable();
            newCitiesAfterInsertion = AddAllCities(newAreasAfterInsertion);

                    ////2.3 all RCs---retrieval and insertion
            System.Data.DataTable newRCsAfterInsertion = new System.Data.DataTable();
            newRCsAfterInsertion = AddAllResearchUnits(newCitiesAfterInsertion);

                    ////2.4 all Labs---retrieval and insertion
            System.Data.DataTable newLabsAfterInsertion = new System.Data.DataTable();
           // newLabsAfterInsertion = AddAllLabs(newRCsAfterInsertion);
            
        }
        protected DataTable AddAllAreas(DataTable legacyAreasBeforeInsertion)

        {
            ApiRequest request = new ApiRequest();
            ApiContent content = new ApiContent();

            request.ApiKey = API_KEY;
            DataTable newAreasAfterInsertion = new DataTable();
            newAreasAfterInsertion.Columns.Add("UmbracoId");
            newAreasAfterInsertion.Columns.Add("Name");
            newAreasAfterInsertion.Columns.Add("ModeCode");

            for (int i = 0; i < legacyAreasBeforeInsertion.Rows.Count; i++)
            {
                string areaName = legacyAreasBeforeInsertion.Rows[i].Field<string>(1);
                string completeModeCode = legacyAreasBeforeInsertion.Rows[i].Field<string>(0);
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
               // properties.Add(new ApiProperty("umbracoUrlName", areaName.Replace(" Area", "").Replace(" ", "-").ToLower())); // sitepublisher ID (So we can reference it later if needed).






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
                                newAreasAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value);
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
                    string cityNameWithStateCode = cityNameWithStateName.Split(',')[0]+'-'+ stateCode;
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



            ////3.all cities--retrieval of new areas umbraco ids,retrieval of old cities based on those modecodes,insertion of new cities

            System.Data.DataTable legacyResearchUnitsBeforeInsertion = new System.Data.DataTable();
            for (int i = 0; i < newCitiesAfterInsertion.Rows.Count; i++)
            {

                int parentCityUmbracoId = Convert.ToInt32(newCitiesAfterInsertion.Rows[i].Field<string>(0));
                string parentLocationModeCode = newCitiesAfterInsertion.Rows[i].Field<string>(2);
                if (parentLocationModeCode.Length < 11)
                    parentLocationModeCode = "0" + parentLocationModeCode;
                legacyResearchUnitsBeforeInsertion = AddRetrieveLocationsDL.GetAllResearchUnits(Convert.ToInt32(parentLocationModeCode.Substring(0, 2)), Convert.ToInt32(parentLocationModeCode.Substring(3, 2)));
                for (int j = 0; j < legacyResearchUnitsBeforeInsertion.Rows.Count; j++)
                {

                    string rCName = legacyResearchUnitsBeforeInsertion.Rows[j].Field<string>(3);
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
                                                                  //properties.Add(new ApiProperty("state", legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Substring(0,legacyCitiesBeforeInsertion.Rows[i].Field<string>(2).Length -2))); // For example: NY (2 letter state code)
                  
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
                                newResearchUnitsAfterInsertion.Rows.Add(responseContent.Id, responseContent.Name, responseContent.Properties[0].Value);
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



                return newResearchUnitsAfterInsertion;
        }
        protected DataTable AddAllLabs(DataTable newRCsAfterInsertion)

        {


            return newRCsAfterInsertion;

        }


    }
}
