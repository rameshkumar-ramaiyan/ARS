using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string savePath = ConfigurationManager.AppSettings.Get("Usda:FileUploadPath");
                string fullSavePath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/" + savePath);

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

                var fileName = result.FileData.Aggregate(string.Empty, (current, file) => current + ("," + file.Headers.ContentDisposition.FileName));

                foreach (FileInfo file in uploadFolder.GetFiles())
                {
                    System.IO.File.Move(file.FullName, fullSavePath + "\\megascheduleIII.mdb");
                }

                foreach (FileInfo file in uploadFolder.GetFiles())
                {
                    FileInfo fileInfo = new FileInfo(fullSavePath + "\\" + file.Name);

                    if (fileInfo != null && fileInfo.Extension == ".mdb")
                    {
                        PullDataFromAccess pullDataFromAccess = new PullDataFromAccess();

                        string html = pullDataFromAccess.SetValues("Provider=Microsoft.ACE.OLEDB.12.0;data source=" + fileInfo.FullName, false);

                        IContent content = _contentService.GetById(Convert.ToInt32(result.FormData["nodeId"]));

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
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, "");
        }
    }
}
