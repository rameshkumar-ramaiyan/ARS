using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Nodes
    {
        public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);

        public static IPublishedContent Homepage()
        {
            return UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("Homepage"));
        }

        public static IPublishedContent SiteSettings()
        {
            return UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings"));
        }

        public static IEnumerable<IPublishedContent> MainNavigationList()
        {
            IPublishedContent mainNav = SiteSettings().Children.FirstOrDefault(n => n.IsDocumentType("MainNavigation"));

            return mainNav.Children;
        }

        public static IEnumerable<IPublishedContent> RegionSiteList()
        {
            IPublishedContent ArsLocations = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("ARSLocations"));

            return ArsLocations.Children;
        }

    }
}
