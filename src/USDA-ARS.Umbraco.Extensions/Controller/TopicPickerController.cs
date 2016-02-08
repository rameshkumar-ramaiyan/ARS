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
    public class TopicPickerController : UmbracoApiController
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
                List<TopicPickerItem> selectList = new List<TopicPickerItem>();

                string textStr = "";

                if (node != null)
                {
                    if (node.DocumentTypeAlias == "PersonSite")
                    {
                        node = node.Parent.Parent;
                    }

                    if (node.DocumentTypeAlias == "ResearchUnit")
                    {
                        if (true == node.HasValue("customTopicTitle") && false == string.IsNullOrWhiteSpace(node.GetPropertyValue<string>("customTopicTitle")))
                        {
                            textStr = node.GetPropertyValue<string>("customTopicTitle") + " - " + node.Name;
                            selectList.Add(new TopicPickerItem(node.Id.ToString(), textStr));
                        }
                    }

                    IEnumerable<IPublishedContent> subNodeList = node.Descendants();

                    foreach (IPublishedContent subNode in subNodeList)
                    {
                        if (true == subNode.HasValue("customTopicTitle") && false == string.IsNullOrWhiteSpace(subNode.GetPropertyValue<string>("customTopicTitle")))
                        {
                            textStr = subNode.GetPropertyValue<string>("customTopicTitle") + " - " + subNode.Name;
                            selectList.Add(new TopicPickerItem(subNode.Id.ToString(), textStr));
                        }
                    }

                    output = JsonConvert.SerializeObject(selectList);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda Topic Picker Error", ex);
            }

            return output;
        }
    }

    public class TopicPickerItem
    {
        public string Value { get; set; }
        public string Text { get; set; }

        public TopicPickerItem(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }
    }
}
