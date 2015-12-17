using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class DataImporterController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        public async Task<HttpResponseMessage> Post()
        {
            bool success = false;
            int nodeId = 0;
            string tableNameDefault = "KeyDates";
            string tableName = "";
            string fullSavePath = null;

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string savePath = ConfigurationManager.AppSettings.Get("Usda:FileUploadPath");
                fullSavePath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/" + savePath);
                

                if (false == System.IO.Directory.Exists(fullSavePath))
                {
                    System.IO.Directory.CreateDirectory(fullSavePath);
                }

                System.IO.DirectoryInfo uploadFolder = new DirectoryInfo(fullSavePath);
                foreach (FileInfo file in uploadFolder.GetFiles())
                {
                    file.Delete();
                }

                var provider = new MultipartFormDataStreamProvider(fullSavePath);
                var result = await Request.Content.ReadAsMultipartAsync(provider);

                nodeId = Convert.ToInt32(result.FormData["nodeId"]);

                var fileName = result.FileData.Aggregate(string.Empty, (current, file) => current + ("," + file.Headers.ContentDisposition.FileName));

                if (result.FormData["tableName"] != null && result.FormData.Get("tableName").Length > 0)
                {
                    tableName = result.FormData.Get("tableName");
                }

                foreach (FileInfo file in uploadFolder.GetFiles())
                {
                    System.IO.File.Move(file.FullName, fullSavePath + "\\data-import-file.mdb");
                }

                foreach (FileInfo file in uploadFolder.GetFiles())
                {
                    FileInfo fileInfo = new FileInfo(fullSavePath + "\\" + file.Name);

                    if (fileInfo != null && fileInfo.Extension == ".mdb")
                    {
                        PullDataFromAccess pullDataFromAccess = new PullDataFromAccess();

                        string html = pullDataFromAccess.SetValues("Provider=Microsoft.ACE.OLEDB.12.0;data source=" + fileInfo.FullName, tableName, false, true);

                        html = Regex.Replace(html, "<body[^>]*>", "");
                        html = Regex.Replace(html, "</body>", "");
                        html = Regex.Replace(html, "</html>", "");

                        IContent content = _contentService.GetById(nodeId);

                        content.SetValue("contentTable", html);

                        _contentService.Save(content);

                        string htmlResponse = "<html><script>window.parent.location.reload();</script></html>";

                        var resp = new HttpResponseMessage(HttpStatusCode.OK);
                        resp.Content = new StringContent(htmlResponse, System.Text.Encoding.UTF8, "text/html");

                        return resp;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Data Import Post Error", ex);

                string errorMessage = "Unknown error. Trust me, we tried finding an error.";

                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    errorMessage = ex.InnerException.Message;
                }
                else if (ex.Message != null)
                {
                    errorMessage = ex.Message;
                }

                errorMessage += " || Input Table Name: " + tableName;

                string redirectScript = "window.parent.location.reload();";

                //if (nodeId > 0)
                //{
                //    redirectScript = "self.location.href='/App_Plugins/UsdaDataImporter/upload.file.aspx?id="+ nodeId +"&ch="+ DateTime.Now.ToString("yyMMddhhmmss") +"';";
                //}

                string htmlResponse = "<html><script>window.parent.alert('ERROR:"+ errorMessage.Replace("'","\"") + "');"+ redirectScript + "</script></html>";

                if (false == string.IsNullOrEmpty(fullSavePath))
                {
                    System.IO.DirectoryInfo uploadFolder = new DirectoryInfo(fullSavePath);
                    foreach (FileInfo file in uploadFolder.GetFiles())
                    {
                        file.Delete();
                    }
                }

                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(htmlResponse, System.Text.Encoding.UTF8, "text/html");

                return resp;
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        }
    }
}
