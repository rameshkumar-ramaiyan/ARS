using Archetype.Models;
using RJP.MultiUrlPicker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Topics
    {
        public static ArchetypeModel GetTopicsByGuid(Guid id, IEnumerable<IPublishedContent> nodeList)
        {

            if (nodeList != null && nodeList.Any())
            {
                foreach (IPublishedContent node in nodeList)
                {
                    if (true == node.HasProperty("leftNavCreate") && true == node.HasValue("leftNavCreate"))
                    {
                        ArchetypeModel topicLinks = node.GetPropertyValue<ArchetypeModel>("leftNavCreate");

                        if (topicLinks != null)
                        {
                            foreach (var topic in topicLinks)
                            {
                                if (topic.Id == id)
                                {
                                    if (true == topic.HasValue("customLeftNav"))
                                    {
                                        return topic.GetValue<ArchetypeModel>("customLeftNav");
                                    }
                                }
                            }
                        }
                    }

                }
            }

            return null;
        }


        public static void GetSiteGlobalNav(IPublishedContent currentNode, ref List<ArchetypeModel> navTopList, ref List<ArchetypeModel> navBottomList)
        {
            IPublishedContent settingsNode = Helpers.Nodes.SiteSettings();

            if (settingsNode != null && currentNode != null)
            {
                ArchetypeModel siteNavs = settingsNode.GetPropertyValue<ArchetypeModel>("siteGlobalLeftNav");

                if (siteNavs != null)
                {
                    foreach (var navItem in siteNavs)
                    {
                        IEnumerable<Link> links = navItem.GetValue<MultiUrls>("siteLeftNavLocation");
                        bool navFound = false;
                        bool forceNavToBottom = false;

                        string navCategorySelect = navItem.GetValue<string>("leftNavCategory");
                        string templateSelect = navItem.GetValue<string>("leftNavTemplate");
                        string docTypeSelect = navItem.GetValue<string>("leftNavDocType");

                        if (false == string.IsNullOrEmpty(navCategorySelect))
                        {
                            if (currentNode.HasValue("navigationCategory") && currentNode.GetPropertyValue<string>("navigationCategory").ToLower() == navCategorySelect.ToLower())
                            {
                                navFound = true;
                            }
                            if (currentNode.HasValue("navigationCategoryBottom") && currentNode.GetPropertyValue<string>("navigationCategoryBottom").ToLower() == navCategorySelect.ToLower())
                            {
                                navFound = true;
                                forceNavToBottom = true;
                            }
                        }
                        else if (false == string.IsNullOrEmpty(templateSelect))
                        {
                            if (currentNode.TemplateId == Convert.ToInt32(templateSelect))
                            {
                                navFound = true;
                            }
                        }
                        else if (false == string.IsNullOrEmpty(docTypeSelect))
                        {
                            if (currentNode.DocumentTypeAlias == docTypeSelect)
                            {
                                navFound = true;
                            }
                        }
                        else
                        {
                            if (links != null && links.Any())
                            {
                                Link foundLink = links.Where(p => p.Url.ToLower() == currentNode.Url.ToLower()).FirstOrDefault();

                                if (foundLink != null)
                                {
                                    navFound = true;
                                }

                                if (false == navFound && true == navItem.GetValue<bool>("displayOnChildPages"))
                                {
                                    foreach (IPublishedContent testAncestor in currentNode.Ancestors())
                                    {
                                        if (false == navFound)
                                        {
                                            Link foundLinkParent = links.Where(p => p.Url.ToLower() == testAncestor.Url.ToLower()).FirstOrDefault();

                                            if (foundLinkParent != null)
                                            {
                                                navFound = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (true == navFound)
                        {
                            bool displayOnBottom = navItem.GetValue<bool>("displayOnBottom");

                            ArchetypeModel navListFound = navItem.GetValue<ArchetypeModel>("siteLeftNav");

                            if (false == displayOnBottom && false == forceNavToBottom)
                            {
                                if (navTopList == null)
                                {
                                    navTopList = new List<ArchetypeModel>();
                                }

                                navTopList.Add(navListFound);
                            }
                            else
                            {
                                if (navBottomList == null)
                                {
                                    navBottomList = new List<ArchetypeModel>();
                                }

                                navBottomList.Add(navListFound);
                            }
                        }
                    }
                }
            }
        }
    }
}
