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
    public partial class delete_regions : System.Web.UI.Page
    {
        protected string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");
        protected string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Delete_Click(object sender, EventArgs e)
        {
            Models.Import.Request request = new Models.Import.Request();
            Models.Import.Content content = new Models.Import.Content();

            request.ApiKey = API_KEY;

            content.Id = 1111; // ARS Locations

            request.ContentList = new List<Models.Import.Content>();
            request.ContentList.Add(content);

            Models.Import.Response responseBack = PostData(request, "Get");

            if (responseBack != null)
            {
                output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                output.Text += "<br />\r\n";

                if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                {
                    Models.Import.Content reqContent = responseBack.ContentList[0];

                    if (reqContent.ChildContentList != null && reqContent.ChildContentList.Count > 0)
                    {
                        foreach (Models.Import.Content child in reqContent.ChildContentList)
                        {
                            output.Text += "Content Id: " + child.Id + "<br />\r\n";
                            output.Text += "Content Name: " + child.Name + "<br />\r\n";
                            output.Text += "<br />\r\n";
                        }

                        Models.Import.Response responseDeleteBack = DeleteNodes(reqContent.ChildContentList);

                        output.Text = "<hr />\r\n";
                        output.Text = "<strong>Delete</strong><br />\r\n";
                        output.Text = "<hr />\r\n";

                        if (responseDeleteBack != null)
                        {
                            output.Text = "Success: " + responseBack.Success + "<br />\r\n";
                            output.Text += "Message: " + responseBack.Message + "<br />\r\n";
                            output.Text += "<br />\r\n";

                            if (responseBack.ContentList != null && responseBack.ContentList.Count > 0)
                            {
                                foreach (Models.Import.Content child in responseBack.ContentList)
                                {
                                    output.Text += "Success: " + child.Success   + "<br />\r\n";
                                    output.Text += "Content Id: " + child.Id + "<br />\r\n";
                                    output.Text += "Content Name: " + child.Name + "<br />\r\n";
                                    output.Text += "<br />\r\n";
                                }

                            }
                        }
                    }
                }
            }
        }


        protected Models.Import.Response DeleteNodes(List<Models.Import.Content> contentList)
        {
            Models.Import.Response response = null;
            Models.Import.Request request = new Models.Import.Request();

            request.ApiKey = API_KEY;
            request.ContentList = contentList;

            response = PostData(request, "Delete");

            return response;
        }



        /// <summary>
        /// Posts a JSON Content Object to the Umbraco API
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Models.Import.Response PostData(Models.Import.Request request, string endPoint = "Post")
        {
            Models.Import.Response response = null;
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

            var httpResponse = http.GetResponse();

            var stream = httpResponse.GetResponseStream();
            var sr = new StreamReader(stream);
            var httpResponseStr = sr.ReadToEnd();

            response = JsonConvert.DeserializeObject<Models.Import.Response>(httpResponseStr);

            return response;
        }
    }
}