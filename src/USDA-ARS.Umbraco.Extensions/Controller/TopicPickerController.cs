using Archetype.Models;
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

                IContent node = ApplicationContext.Current.Services.ContentService.GetById(Convert.ToInt32(id));
                List<TopicPickerItem> selectList = new List<TopicPickerItem>();

                if (node != null)
                {

                    if (node.ContentType.Alias == "PersonSite")
                    {
                        node = node.Parent().Parent();
                    }

                    if (node.ContentType.Alias != "ARSLocations" && node.ContentType.Alias != "PersonSite")
                    {
                        IContent checkNode = node.Ancestors().FirstOrDefault();

                        if (checkNode.ContentType.Alias == "Homepage")
                        {
                            node = node.Ancestors().FirstOrDefault();
                        }
                        else
                        {

                        }

                        selectList.AddRange(GetTopicList(node));
                    }

                    if (node.ContentType.Alias == "ARSLocations")
                    {
                        selectList.AddRange(GetTopicList(node));
                    }
                    else
                    {
                        IEnumerable<IContent> subNodeList = node.Descendants();

                        foreach (IContent subNode in subNodeList)
                        {
                            selectList.AddRange(GetTopicList(subNode));
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


        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string GetTitle(string id)
        {
            string output = "";

            UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

            IEnumerable<IContent> nodeList = ApplicationContext.Current.Services.ContentService.GetRootContent();

            foreach (IContent node in nodeList)
            {
                List<TopicPickerItem> pickerList = GetTopicList(node);

                TopicPickerItem pickerItem = pickerList.Where(p => p.Value.ToLower() == id.ToLower()).FirstOrDefault();

                if (pickerItem != null)
                {
                    return pickerItem.Text;
                }
            }

            return output;
        }


        private List<TopicPickerItem> GetTopicList(IContent node)
        {
            List<TopicPickerItem> pickerList = new List<TopicPickerItem>();

            if (node.HasProperty("leftNavCreate") && node.GetValue("leftNavCreate") != null && false == string.IsNullOrEmpty(node.GetValue("leftNavCreate").ToString()))
            {
                string archetypeStrValue = node.GetValue("leftNavCreate").ToString();

                ArchetypeModel topicLinks = JsonConvert.DeserializeObject<ArchetypeModel>(archetypeStrValue);

                if (topicLinks != null && topicLinks.Any())
                {
                    foreach (var topic in topicLinks)
                    {
                        if (topic.HasValue("customLeftNavTitle"))
                        {
                            string textStr = topic.GetValue("customLeftNavTitle") + " - " + node.Name;

                            pickerList.Add(new TopicPickerItem(topic.Id.ToString(), textStr));
                        }
                    }
                }
            }

            return pickerList;
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
