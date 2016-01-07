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
            Models.Import.Content content = new Models.Import.Content();

            content.ApiKey = API_KEY;
            content.Id = 0; // New page
            content.Name = "Test Page";
            content.ParentId = 1111;
            content.DocType = "Region";
            content.Template = "Region";

            List<Models.Import.Property> properties = new List<Models.Import.Property>();

            properties.Add(new Models.Import.Property("modeCode", "90-00-00-00")); // Region mode code
            properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=50-00-00-00")); // current URL
            properties.Add(new Models.Import.Property("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).

            content.Properties = properties;

            content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            Models.Import.Response responseBack = PostData(content);

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }
        }


        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            Models.Import.Content content = new Models.Import.Content();

            content.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtIdUpdate.Text); // Update a page
            content.Name = txtName.Text;

            List<Models.Import.Property> properties = new List<Models.Import.Property>();

            properties.Add(new Models.Import.Property("modeCode", "80-00-00-00")); // Region mode code
            properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=80-00-00-00")); // current URL
            properties.Add(new Models.Import.Property("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).

            content.Properties = properties;

            content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            Models.Import.Response responseBack = PostData(content);

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }
        }


        protected void btnGet_Click(object sender, EventArgs e)
        {
            Models.Import.Content content = new Models.Import.Content();

            content.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtId.Text); // Load page

            Models.Import.Response responseBack = PostData(content, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                    output.Text += "<strong>Properties</strong><br />\r\n";

                    foreach (Models.Import.Property property in responseBack.Content.Properties)
                    {
                        output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                    }
                }
            }
        }


        protected void btnGetByModeCode_Click(object sender, EventArgs e)
        {
            Models.Import.Content content = new Models.Import.Content();

            content.ApiKey = API_KEY;
            content.Id = 0;
            content.Properties = new List<Models.Import.Property>();
            content.Properties.Add(new Models.Import.Property("modeCode", txtModeCode.Text.ToString())); // Load page by property value

            Models.Import.Response responseBack = PostData(content, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                    output.Text += "<strong>Properties</strong><br />\r\n";

                    foreach (Models.Import.Property property in responseBack.Content.Properties)
                    {
                        output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                    }
                }

                
            }
        }


        protected void btnGetChild_Click(object sender, EventArgs e)
        {
            Models.Import.Content content = new Models.Import.Content();

            content.ApiKey = API_KEY;
            content.Id = Convert.ToInt32(txtParentId.Text); // Load page

            Models.Import.Response responseBack = PostData(content, "GetChildsList");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                    output.Text += "<strong>Properties</strong><br />\r\n";

                    foreach (Models.Import.Property property in responseBack.Content.Properties)
                    {
                        output.Text += property.Key + ": " + property.Value + " <br />\r\n";
                    }

                    if (responseBack.ChildContentList != null)
                    {
                        outputChild.Text = "";

                        foreach (Models.Import.Content childContent in responseBack.ChildContentList)
                        {
                            outputChild.Text += " - Child Content Umbraco Id: " + childContent.Id + "<br />\r\n";
                            outputChild.Text += " - Child Content Name: " + childContent.Name + "<br /><br />\r\n";
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Posts a JSON Content Object to the Umbraco API
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Models.Import.Response PostData(Models.Import.Content content, string endPoint = "Post")
        {
            Models.Import.Response response = null;
            string apiUrl = API_URL;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl + endPoint));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            string parsedContent = JsonConvert.SerializeObject(content);
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var httpResponse = http.GetResponse();

            var stream = httpResponse.GetResponseStream();
            var sr = new StreamReader(stream);
            var httpResponseStr = sr.ReadToEnd();

            response = JsonConvert.DeserializeObject<Models.Import.Response>(httpResponseStr);

            return response;
        }

          protected void btnAddNewArea_Click(object sender, EventArgs e)
        {
             Models.Import.Content content = new Models.Import.Content();
            content.ApiKey = API_KEY;


                string oldId = txtOldId.Text;

                content.Id = 0; // New page
                content.Name = txtParentAreaName.Text;
                content.ParentId = 1111;
                content.DocType = "Region";
                content.Template = "Region";

                List<Models.Import.Property> properties = new List<Models.Import.Property>();
                string newModeCodeProperty = txtNewModeCode.Text;
                string oldModeCodeProperty = txtOldModeCode.Text;

                properties.Add(new Models.Import.Property("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=" + newModeCodeProperty + "")); // current URL               
                properties.Add(new Models.Import.Property("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).


                //properties.Add(new Models.Import.Property("modeCode", "90-00-00-00")); // Region mode code
                //properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=50-00-00-00")); // current URL
                // properties.Add(new Models.Import.Property("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).



                content.Properties = properties;
                content.Save = 2; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish
            
           
           

            

            Models.Import.Response responseBack = PostData(content);

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }
        }

        protected void btnAddNewCity_Click(object sender, EventArgs e)
        {
            // === ADD NEW CITY ===
            // Set the parent ID. You will need to get the Content ID for the Area the city is under.

            Models.Import.Content content = new Models.Import.Content();
            content.ApiKey = API_KEY;

            string oldId = txtCityOldIdSP.Text;
            string stateCode = txtStateCode.Text.ToUpper();
            string cityName = txtCityName.Text.ToUpper();
            string cityStateConcatenatedString = cityName + "," + stateCode;
            content.Id = 0; // New page
             // content.Name = "{City Name, State Code}";
            //content.ParentId = { The Umbraco Content ID for the AREA};
            content.Name = cityStateConcatenatedString;
            content.ParentId =Convert.ToInt32(txtParentAreaId.Text);
            content.DocType = "City";
            content.Template = ""; // Leave blank

            List<Models.Import.Property> properties = new List<Models.Import.Property>();
            string newModeCodeProperty = txtParentAreaModeCode.Text;
            string oldModeCodeProperty = txtOldModeCode.Text;

            properties.Add(new Models.Import.Property("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
            properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=" + newModeCodeProperty + "")); // current URL               
            properties.Add(new Models.Import.Property("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
            properties.Add(new Models.Import.Property("state",txtStateCode.Text)); // For example: NY (2 letter state code)
            properties.Add(new Models.Import.Property("navigationTitle", cityStateConcatenatedString)); // All CAPS - For example: GENEVA, NY

            //properties.Add(new Models.Import.Property("modeCode", "80-10-00-00")); // Region mode code
            //properties.Add(new Models.Import.Property("oldUrl", "")); // Leave blank since there is no city page on the website.
            //properties.Add(new Models.Import.Property("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
            //properties.Add(new Models.Import.Property("state", "{State Code}")); // For example: NY (2 letter state code)
            //properties.Add(new Models.Import.Property("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            Models.Import.Response responseBack = PostData(content, "Post");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }

        }

        protected void btnAddNewRC_Click(object sender, EventArgs e)
        {
            // === ADD NEW LAB ===
            // Set the parent ID. You will need to get the Content ID for the Area the city is under.

            Models.Import.Content content = new Models.Import.Content();
            content.ApiKey = API_KEY;

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

            List<Models.Import.Property> properties = new List<Models.Import.Property>();
            string newModeCodeProperty = txtParentAreaModeCode.Text;
            string oldModeCodeProperty = txtOldModeCode.Text;

            properties.Add(new Models.Import.Property("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
            properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=" + newModeCodeProperty + "")); // current URL               
            properties.Add(new Models.Import.Property("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).
            properties.Add(new Models.Import.Property("state", txtStateCode.Text)); // For example: NY (2 letter state code)
            properties.Add(new Models.Import.Property("navigationTitle", cityStateConcatenatedString)); // All CAPS - For example: GENEVA, NY

            //properties.Add(new Models.Import.Property("modeCode", "80-10-00-00")); // Region mode code
            //properties.Add(new Models.Import.Property("oldUrl", "")); // Leave blank since there is no city page on the website.
            //properties.Add(new Models.Import.Property("oldId", "1234")); // NOT REQUIRED. INTERNAL USE ONLY. sitepublisher ID (So we can reference it later if needed).
            //properties.Add(new Models.Import.Property("state", "{State Code}")); // For example: NY (2 letter state code)
            //properties.Add(new Models.Import.Property("navigationTitle", "{City Name, State Code}")); // All CAPS - For example: GENEVA, NY

            content.Properties = properties;

            content.Save = 2; // 1=Saved, 2=Save And Publish

            Models.Import.Response responseBack = PostData(content, "Post");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }
        }
    }
}