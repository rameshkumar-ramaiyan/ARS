using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;
using USDA_ARS.LocationsWebApp.DL;
using System.Net;
using Newtonsoft.Json;


namespace USDA_ARS.LocationsWebApp
{
    public partial class AddRetrieveLocations : System.Web.UI.Page
    {
        protected string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        protected string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnAreaRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allAreas = new System.Data.DataTable();
            allAreas = AddRetrieveLocationsDL.GetAllAreas();
            lblMessage.Text = allAreas.Rows.Count.ToString() + " Areas retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allAreas;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }

        protected void btnCityRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allCities = new System.Data.DataTable();
            int parentAreaModeCode =0;
            if (!string.IsNullOrEmpty(txtParentAreaModeCode.Text))
            {
                 parentAreaModeCode = Convert.ToInt32(txtParentAreaModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent Area Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            allCities = AddRetrieveLocationsDL.GetAllCities(parentAreaModeCode);
            lblMessage.Text = allCities.Rows.Count.ToString() + " Cities retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allCities;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }

        protected void btnResearchCenterRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allResearchCenters = new System.Data.DataTable();
            int parentAreaModeCode =0;
            int parentCityModeCode =0;
            int parentResearchCenterModeCode = 0;
            //txtParentAreaModeCode
            if (!string.IsNullOrEmpty(txtRCParentAreaModeCode.Text))
            {
                parentAreaModeCode = Convert.ToInt32(txtRCParentAreaModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent Area Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            //txtRCParentCityModeCode
            if (!string.IsNullOrEmpty(txtRCParentCityModeCode.Text))
            {
                parentCityModeCode = Convert.ToInt32(txtRCParentCityModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent City Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            //txtParenRCModeCode
            if (!string.IsNullOrEmpty(txtParenRCModeCode.Text))
            {
                parentResearchCenterModeCode = Convert.ToInt32(txtParenRCModeCode.Text);
            }
            
            allResearchCenters = AddRetrieveLocationsDL.GetAllResearchCenters(parentAreaModeCode, parentCityModeCode, parentResearchCenterModeCode);
            lblMessage.Text = allResearchCenters.Rows.Count.ToString() + " Research Centers retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allResearchCenters;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }

        protected void btnAddNewArea_Click(object sender, EventArgs e)
        {
             Models.Import.Content content = new Models.Import.Content();
            content.ApiKey = API_KEY;


            string oldId = txtOldId.Text;

            if (string.IsNullOrEmpty(oldId) || string.IsNullOrWhiteSpace(oldId) || oldId == "0")
            {
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
                content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish
            }
            else
            {
                content.Id = Convert.ToInt32(oldId); // Update page
                content.Name = txtParentAreaName.Text;
                content.ParentId = 1111;
                content.DocType = "Region";
                content.Template = "Region";

                List<Models.Import.Property> properties = new List<Models.Import.Property>();
                string newModeCodeProperty = txtNewModeCode.Text;
                string oldModeCodeProperty = txtOldModeCode.Text;

                properties.Add(new Models.Import.Property("modeCode", newModeCodeProperty)); // Region mode code                                                                                            
                properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=" + oldModeCodeProperty + "")); // current URL               
                properties.Add(new Models.Import.Property("oldId", oldId)); // sitepublisher ID (So we can reference it later if needed).


                //properties.Add(new Models.Import.Property("modeCode", "90-00-00-00")); // Region mode code
                //properties.Add(new Models.Import.Property("oldUrl", "/main/site_main.htm?modeCode=50-00-00-00")); // current URL
                // properties.Add(new Models.Import.Property("oldId", "1234")); // sitepublisher ID (So we can reference it later if needed).



                content.Properties = properties;
                content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

            }

            

            Models.Import.Response responseBack = PostData(content);

            if (responseBack != null)
            {
                output1.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output1.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output1.Text += "<br />\r\n";

                if (responseBack.Content != null)
                {
                    output1.Text += "Content Umbraco Id: " + responseBack.Content.Id + "<br />\r\n";
                    output1.Text += "Content Name: " + responseBack.Content.Name + "<br />\r\n";
                }
            }
        }
        /// <summary>
        /// Posts a JSON Content Object to the Umbraco API
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Models.Import.Response PostData(Models.Import.Content content)
        {
            Models.Import.Response response = null;
            string apiUrl = API_URL;

            var http = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl));
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

       
    }
}