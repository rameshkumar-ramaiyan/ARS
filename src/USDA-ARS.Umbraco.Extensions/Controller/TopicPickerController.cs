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

                if (node != null)
                {

                    if (node.ContentType.Alias == "PersonSite")
                    {
                        // node
                    }

                    if (node.ContentType.Alias != "ARSLocations" && node.ContentType.Alias != "PersonSite")
                    {
                        IPublishedContent checkNode = node.AncestorsOrSelf(1).FirstOrDefault();

                        if (checkNode.ContentType.Alias == "Homepage")
                        {
                            node = node.Ancestors().FirstOrDefault();
                        }
                        else
                        {
                            if (node.Parent.ContentType.Alias == "ResearchUnit")
                            {
                                node = node.Parent;
                            }
                            else if (node.Parent.Parent.ContentType.Alias == "ResearchUnit")
                            {
                                node = node.Parent.Parent;
                            }
                            else
                            {
                                IPublishedContent checkNode3 = node.AncestorsOrSelf(2).FirstOrDefault();

                                if (checkNode3.ContentType.Alias == "Area")
                                {
                                    node = checkNode3;
                                }
                            }
                        }

                        selectList.AddRange(GetTopicList(node));
                    }

                    if (node.ContentType.Alias == "ARSLocations")
                    {
                        selectList.AddRange(GetTopicList(node));
                    }
                    else
                    {
                        List<IPublishedContent> subNodeList = new List<IPublishedContent>();
                        IPublishedContent tempNode = node;

                        int level = tempNode.Level;

                        if (level > 2)
                        {
                            while (level > 2)
                            {
                                tempNode = tempNode.Parent;
                                level = tempNode.Level;

                                subNodeList.Add(tempNode);
                            }
                        }

                        subNodeList.AddRange(node.Descendants());

                        foreach (IPublishedContent subNode in subNodeList)
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


        private List<TopicPickerItem> GetTopicList(IPublishedContent node)
        {
            List<TopicPickerItem> pickerList = new List<TopicPickerItem>();

            if (node.HasValue("leftNavCreate"))
            {
                ArchetypeModel topicLinks = node.GetPropertyValue<ArchetypeModel>("leftNavCreate");

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

    

    
}
