using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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

                if ((node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit"))
                {
                    // Docs Folder
                    IContent docsFolder = node.Descendants().FirstOrDefault(n => n.ContentType.Alias == "DocsFolder");

                    if (docsFolder == null)
                    {
                        var childNode = _contentService.CreateContent("Docs", node, "DocsFolder");

                        _contentService.SaveAndPublishWithStatus(childNode);
                    }

                    // Careers Node
                    IContent sitesCareers = node.Descendants().FirstOrDefault(n => n.ContentType.Alias == "SitesCareers");

                    if (sitesCareers == null)
                    {
                        string navCategoryGuid = Helpers.NavCategories.GetNavGuidByName("Careers");

                        var childNode = _contentService.CreateContent("Careers", node, "SitesCareers");
                        childNode.SetValue("navigationCategory", navCategoryGuid);

                        _contentService.SaveAndPublishWithStatus(childNode);
                    }

                    // News Node
                    IContent sitesNews = node.Descendants().FirstOrDefault(n => n.ContentType.Alias == "SitesNews");

                    if (sitesNews == null)
                    {
                        string navCategoryGuid = Helpers.NavCategories.GetNavGuidByName("News");

                        var childNode = _contentService.CreateContent("News", node, "SitesNews");
                        childNode.SetValue("navigationCategory", navCategoryGuid);

                        _contentService.SaveAndPublishWithStatus(childNode);
                    }

                    // People Folder
                    IContent peopleFolder = node.Descendants().FirstOrDefault(n => n.ContentType.Alias == "PeopleFolder");

                    if (peopleFolder == null)
                    {
                        var childNode = _contentService.CreateContent("People", node, "PeopleFolder");

                        _contentService.SaveAndPublishWithStatus(childNode);
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
