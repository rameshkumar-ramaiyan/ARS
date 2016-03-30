using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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
                if ((node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit"))
                {
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
                    IContent peopleFolder = node.Descendants().FirstOrDefault(n => n.ContentType.Alias == "Careers");

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
