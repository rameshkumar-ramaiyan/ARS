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
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class NewsImporterController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public void Go()
        {
            bool success = false;

            try
            {
                List<UsdaArsNews> newsListTemp = USDA_ARS.Umbraco.Extensions.Helpers.News.GetNews(9999);

                foreach (UsdaArsNews item in newsListTemp)
                {
                    var content = _contentService.CreateContent(item.SubjectField, 2421, "NewsItem");

                    content.SetValue("title", item.SubjectField);
                    content.SetValue("bodyText", item.BodyField);
                    content.SetValue("dateField", item.DateField);
                    content.SetValue("newsId", item.NewsID.ToString());
                    _contentService.SaveAndPublishWithStatus(content);

                    LogHelper.Info<DataImporterController>("Node Added: " + item.SubjectField);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Data Import Post Error", ex);
            }
        }
    }
}
