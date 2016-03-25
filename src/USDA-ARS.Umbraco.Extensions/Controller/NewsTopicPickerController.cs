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
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class NewsTopicPickerController : UmbracoApiController
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
                IPublishedContent node = Helpers.Nodes.SiteSettings();
                List<NewsTopicItem> selectList = new List<NewsTopicItem>();

                if (node != null)
                {
                    if (node.HasValue("newsTopics"))
                    {
                        ArchetypeModel newsTopics = node.GetPropertyValue<ArchetypeModel>("newsTopics");

                        if (newsTopics != null && newsTopics.Any())
                        {
                            foreach (var newsTopic in newsTopics)
                            {
                                string newsTopicTitle = newsTopic.GetValue<string>("newsTopic");

                                NewsTopicItem newsTopicItem = new NewsTopicItem();

                                newsTopicItem.Value = newsTopic.Id.ToString();
                                newsTopicItem.Text = newsTopicTitle;

                                selectList.Add(newsTopicItem);
                            }

                        }

                        output = JsonConvert.SerializeObject(selectList);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda News Topic Picker Error", ex);
            }

            return output;
        }


        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string GetTitle(string id)
        {
            string output = "";

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            IPublishedContent node = Helpers.Nodes.SiteSettings();
            List<NewsTopicItem> selectList = new List<NewsTopicItem>();

            if (node != null)
            {
                if (node.HasValue("newsTopics"))
                {
                    ArchetypeModel newsTopics = node.GetPropertyValue<ArchetypeModel>("newsTopics");

                    var newsTopicItem = newsTopics.Where(p => p.Id.ToString().ToLower() == id.ToLower()).FirstOrDefault();

                    if (newsTopicItem != null)
                    {
                        output = newsTopicItem.GetValue<string>("");
                    }
                }
            }


                    //UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

                    //IEnumerable<IPublishedContent> nodeList = umbracoHelper.TypedContentAtRoot();

                    //foreach (IPublishedContent node in nodeList)
                    //{
                    //    List<TopicPickerItem> pickerListParent = GetTopicList(node);

                    //    TopicPickerItem pickerItemParent = pickerListParent.Where(p => p.Value.ToLower() == id.ToLower()).FirstOrDefault();

                    //    if (pickerItemParent != null)
                    //    {
                    //        return pickerItemParent.Text;
                    //    }

                    //    foreach (IPublishedContent subNode in node.Descendants())
                    //    {
                    //        List<TopicPickerItem> pickerList = GetTopicList(subNode);

                    //        TopicPickerItem pickerItem = pickerList.Where(p => p.Value.ToLower() == id.ToLower()).FirstOrDefault();

                    //        if (pickerItem != null)
                    //        {
                    //            return pickerItem.Text;
                    //        }
                    //    }
                    //}

                    return output;
        }

    }




}
