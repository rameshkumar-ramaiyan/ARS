using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class ImageCheckController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]

        public string Get()
        {
            string output = "";

            try
            {
                string filePath = "";

                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["imagePath"]))
                {
                    filePath = HttpContext.Current.Request.QueryString.Get("imagePath");
                    string fullFilePath = HttpContext.Current.Server.MapPath(filePath);

                    if (true == System.IO.File.Exists(fullFilePath))
                    {
                        Image img = Image.FromFile(fullFilePath);

                        if (img != null)
                        {
                            if (img.Width != 520 && img.Height != 320)
                            {
                                output = "<br /><strong style=\"\">Incorrect resolution. Not 520x320</strong>";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda Download Request Error", ex);
            }

            return output;
        }
    }
}
