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
      public static ArchetypeModel GetTopicsById(int id)
      {
         ArchetypeModel topicLinks = null;

         var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
         IPublishedContent navSet = umbracoHelper.TypedContent(id);

         if (navSet != null)
         {
            if (true == navSet.HasValue("navigationItems"))
            {
               topicLinks = navSet.GetPropertyValue<ArchetypeModel>("navigationItems");
            }
         }

         return topicLinks;
      }


      public static void GetSiteGlobalNav(IPublishedContent currentNode, ref List<IPublishedContent> navTopList, ref List<IPublishedContent> navBottomList)
      {
         bool hideGlobalNav = false;

         if (currentNode != null && currentNode.HasProperty("hideGlobalNav") && true == currentNode.GetPropertyValue<bool>("hideGlobalNav"))
         {
            hideGlobalNav = true;
         }

         if (false == hideGlobalNav)
         {
            IPublishedContent settingsNode = Helpers.Nodes.SiteSettings();

            IPublishedContent globalNavFolder = settingsNode.Children(p => p.DocumentTypeAlias == "SiteSettingsNavFolder").FirstOrDefault();

            if (globalNavFolder != null)
            {
               IEnumerable<IPublishedContent> globalNavList = globalNavFolder.Children;

               if (globalNavList != null)
               {
                  foreach (IPublishedContent navItem in globalNavList)
                  {
                     IEnumerable<Link> links = navItem.GetPropertyValue<MultiUrls>("siteLeftNavLocation");
                     bool navFound = false;
                     bool forceNavToBottom = false;

                     string navCategorySelect = navItem.GetPropertyValue<string>("leftNavCategory");
                     string templateSelect = navItem.GetPropertyValue<string>("leftNavTemplate");
                     string docTypeSelect = navItem.GetPropertyValue<string>("leftNavDocType");

                     // If a NP Document, use the NP info
                     if (currentNode.Parent != null && currentNode.Parent.DocumentTypeAlias == "NationalProgramFolderContainer")
                     {
                        currentNode = currentNode.Parent.Parent;
                     }

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
                        }
                     }

                     if (true == navFound)
                     {
                        bool displayOnBottom = navItem.GetPropertyValue<bool>("displayOnBottom");

                        IPublishedContent navListFound = navItem;

                        if (false == displayOnBottom && false == forceNavToBottom)
                        {
                           if (navTopList == null)
                           {
                              navTopList = new List<IPublishedContent>();
                           }

                           navTopList.Add(navListFound);
                        }
                        else
                        {
                           if (navBottomList == null)
                           {
                              navBottomList = new List<IPublishedContent>();
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
}
