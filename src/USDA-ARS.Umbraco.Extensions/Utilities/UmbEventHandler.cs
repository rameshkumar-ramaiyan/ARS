using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;

namespace USDA_ARS.Umbraco.Extensions.Utilities
{
    public class UmbEventHandler : ApplicationEventHandler
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;
        

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saved += PostProcessSave;
            ContentService.Deleted += PostProcessDelete;
        }

        private static void PostProcessSave(IContentService cs, SaveEventArgs<IContent> e)
        {
            foreach (var node in e.SavedEntities)
            {
                // SOFTWARE
                if (node.ContentType.Alias == "Homepage" || node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit")
                {
                    ArchetypeModel software = node.GetValue<ArchetypeModel>("software");

                    string archetypeStr = node.GetValue<string>("software");

                    if (software == null && false == string.IsNullOrEmpty(archetypeStr))
                    {
                        software = JsonConvert.DeserializeObject<ArchetypeModel>(archetypeStr);
                    }

                    bool updateSoftware = false;

                    if (software != null)
                    {
                        foreach (ArchetypeFieldsetModel fieldsetModel in software.Fieldsets)
                        {
                            string softwareId = fieldsetModel.GetValue<string>("softwareID");

                            if (true == string.IsNullOrEmpty(softwareId))
                            {
                                updateSoftware = true;

                                int softwareIdNext = Software.GetLastSoftwareId() + 1;

                                var newSoftwareID = fieldsetModel.Properties.FirstOrDefault(x => x.Alias == "softwareID");

                                if (newSoftwareID != null)
                                {
                                    newSoftwareID.Value = softwareIdNext;
                                }
                            }
                        }

                        if (true == updateSoftware)
                        {
                            node.SetValue("software", JsonConvert.SerializeObject(software));
                            _contentService.SaveAndPublishWithStatus(node);
                        }
                    }
                }

                // SUB FOLDERS FOR REGIONS AND RESEARCH UNITS
                if ((node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit"))
                {
                    UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
                    int siteFolderTemplateNodeId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Usda:SiteFoldesrTemplateNodeId")); //

                    IPublishedContent siteFoldersTemplate = umbHelper.TypedContent(siteFolderTemplateNodeId);

                    if (siteFoldersTemplate != null)
                    {
                        List<IPublishedContent> siteFolderSubNodeList = siteFoldersTemplate.Children.ToList();

                        if (siteFolderSubNodeList != null && siteFolderSubNodeList.Any())
                        {
                            foreach (IPublishedContent subNode in siteFolderSubNodeList)
                            {
                                IContent testSubNode = node.Descendants().FirstOrDefault(n => n.Name == subNode.Name);

                                if (testSubNode == null)
                                {
                                    IContent createSubNode = _contentService.GetById(subNode.Id);

                                    if (createSubNode != null)
                                    {
                                        IContent newSubNode = _contentService.Copy(createSubNode, node.Id, false);

                                        if (newSubNode != null)
                                        {
                                            _contentService.PublishWithChildrenWithStatus(newSubNode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (node.ContentType.Alias == "NationalProgram" && !cs.HasChildren(node.Id))
                {
                    var childNode = _contentService.CreateContent("Docs", node, "NationalProgramFolderContainer");

                    _contentService.SaveAndPublishWithStatus(childNode);
                }
                else if (node.ContentType.Alias == "NewsArticle")
                {
                    int nodeId = node.Id;

                    string bodyText = node.GetValue<string>("bodyText");

                    NewsInterLinks.RemoveLinksByNodeId(nodeId);

                    List<LinkItem> linkItemList = NewsInterLinks.FindInterLinks(bodyText);

                    if (linkItemList != null)
                    {
                        NewsInterLinks.GenerateInterLinks(node, linkItemList);
                    }
                }
            }
        }

        private static void PostProcessDelete(IContentService cs, DeleteEventArgs<IContent> e)
        {
            foreach (var node in e.DeletedEntities)
            {
                if (node.ContentType.Alias == "NewsArticle")
                {
                    NewsInterLinks.RemoveLinksByNodeId(node.Id);
                }
            }
        }
    }
}
