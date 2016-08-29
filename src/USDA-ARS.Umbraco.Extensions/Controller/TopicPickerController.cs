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
            List<IPublishedContent> navFolderList = new List<IPublishedContent>();
            bool restrictToCurrentNode = false;

            if (node != null)
            {
               // Test location 
               IPublishedContent locationNode = node.Ancestors().Where(p => p.IsDocumentType("ResearchUnit") || p.IsDocumentType("Area")).FirstOrDefault();

               if (locationNode != null)
               {
                  restrictToCurrentNode = true;
               }

               navFolderList = GetFolderList(node, restrictToCurrentNode);

               if (navFolderList != null)
               {
                  foreach (IPublishedContent navNode in navFolderList)
                  {
                     selectList.AddRange(GetTopicList(navNode));
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


         IPublishedContent node = Helpers.Nodes.GetNavNodeIdByGuid(id);

         if (node != null)
         {
            output = node.Name + "  //  (" + node.Parent.Parent.Name + ")";
         }


         //try
         //{
         //   var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
         //   IPublishedContent node = umbracoHelper.TypedContent(id);
         //   List<TopicPickerItem> selectList = new List<TopicPickerItem>();
         //   List<IPublishedContent> navFolderList = new List<IPublishedContent>();

         //   if (node != null)
         //   {
         //      navFolderList = GetFolderList(node);

         //      if (navFolderList != null)
         //      {
         //         foreach (IPublishedContent navNode in navFolderList)
         //         {
         //            selectList.AddRange(GetTopicList(navNode));
         //         }

         //         TopicPickerItem pickerItem = selectList.Where(p => p.Value.Value.ToLower() == id.ToLower()).FirstOrDefault();

         //         if (pickerItem != null)
         //         {
         //            output = JsonConvert.SerializeObject(pickerItem);
         //         }
         //      }
         //   }
         //}
         //catch (Exception ex)
         //{
         //   LogHelper.Error<DataImporterController>("Usda Topic Get Title Error", ex);
         //}

         return output;
      }


      private List<TopicPickerItem> GetTopicList(IPublishedContent node)
      {
         List<TopicPickerItem> pickerList = new List<TopicPickerItem>();

         IEnumerable<IPublishedContent> nodeChildrenList = node.Children;

         if (nodeChildrenList != null && nodeChildrenList.Any())
         {
            nodeChildrenList = nodeChildrenList.OrderBy(p => p.Name);

            foreach (var topicNode in nodeChildrenList)
            {
               string textStr = "";

               textStr = topicNode.Name + "  //  (" + topicNode.Parent.Parent.Name + ")";

               pickerList.Add(new TopicPickerItem(topicNode.Id.ToString(), textStr));
            }
         }

         return pickerList;
      }


      private List<IPublishedContent> GetFolderList(IPublishedContent node, bool restrictToCurrentNode = false)
      {
         List<IPublishedContent> navFolderList = new List<IPublishedContent>();
         List<IPublishedContent> nodesList = new List<IPublishedContent>();

         if (true == restrictToCurrentNode)
         {
            if (node.DocumentTypeAlias == "ResearchUnit" || 
                  node.DocumentTypeAlias == "Area" || 
                  node.DocumentTypeAlias == "Homepage" || 
                  node.DocumentTypeAlias == "NationalProgram" || 
                  node.DocumentTypeAlias == "Subsite" || 
                  node.DocumentTypeAlias == "PersonSite")
            {
               nodesList.Add(node);
            }

            node = node.Ancestors().Where(p => p.IsDocumentType("ResearchUnit") || p.IsDocumentType("Area") || p.IsDocumentType("Homepage") ||
                     p.IsDocumentType("NationalProgram") || p.IsDocumentType("Subsite") || p.IsDocumentType("PersonSite")).FirstOrDefault();
            nodesList.Add(node);


         }
         else
         {
            nodesList = node.AncestorsOrSelf<IPublishedContent>().ToList();
         }

         if (node.DocumentTypeAlias == "NationalProgram")
         {
            IPublishedContent homeNode = Helpers.Nodes.Homepage();

            if (homeNode != null)
            {
               nodesList.Insert(0, homeNode);
            }
         }

         if (nodesList != null)
         {
            nodesList = nodesList.OrderBy(p => p.Level).ThenBy(x => x.Name).ToList();

            foreach (IPublishedContent subNode in nodesList)
            {
               IPublishedContent navFolder = subNode.Children.Where(p => p.DocumentTypeAlias == "SiteNavFolder").FirstOrDefault();

               if (navFolder != null)
               {
                  navFolderList.Add(navFolder);
               }
            }

            //if (cu)
         }


         return navFolderList;
      }
   }




}
