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
    public partial class test_import : System.Web.UI.Page
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

            content.Save = 1; // 0=Unpublish (update only), 1=Saved, 2=Save And Publish

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
                        output.Text += property.Key +": " + property.Value + " <br />\r\n";
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
    }
}