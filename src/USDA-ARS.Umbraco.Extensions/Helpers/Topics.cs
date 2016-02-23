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

                        Link foundLink = links.Where(p => p.Url.ToLower() == currentNode.Url.ToLower()).FirstOrDefault();

                        if (foundLink != null)
                        {
                            bool displayOnBottom = navItem.GetValue<bool>("displayOnBottom");

                            ArchetypeModel navListFound = navItem.GetValue<ArchetypeModel>("siteLeftNav");

                            if (false == displayOnBottom)
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
