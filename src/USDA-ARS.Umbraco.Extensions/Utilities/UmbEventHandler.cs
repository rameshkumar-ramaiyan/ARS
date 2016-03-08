using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace USDA_ARS.Umbraco.Extensions.Utilities
{
    public class UmbEventHandler : ApplicationEventHandler
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saved += CreateChildNodes;
        }

        private static void CreateChildNodes(IContentService cs, SaveEventArgs<IContent> e)
        {
            foreach (var node in e.SavedEntities)
            {
                if ((node.ContentType.Alias == "Region" || node.ContentType.Alias == "ResearchUnit") && !cs.HasChildren(node.Id))
                {
                    var childNode = _contentService.CreateContent("People", node, "PeopleFolder");

                    _contentService.SaveAndPublishWithStatus(childNode);
                }
                else if (node.ContentType.Alias == "NationalProgram" && !cs.HasChildren(node.Id))
                {
                    var childNode = _contentService.CreateContent("Docs", node, "NationalProgramFolderContainer");

                    _contentService.SaveAndPublishWithStatus(childNode);
                }
            }
        }
    }
}
