using Newtonsoft.Json;
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
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class DownloadRequestController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]

        public string Get(string id)
        {
            string output = "";

            try
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                IPublishedContent node = umbracoHelper.TypedContent(id);

                if (node != null)
                {
                    Models.NodeDownloadRequests nodeDownloadRequests = new Models.NodeDownloadRequests();

                    nodeDownloadRequests = Helpers.Aris.DownloadRequests.GetDownloadRequestsByNode(node);

                    if (nodeDownloadRequests != null)
                    {
                        output = JsonConvert.SerializeObject(nodeDownloadRequests);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DownloadRequestController>("Usda Download Request Error", ex);
            }

            return output;
        }
    }
}
