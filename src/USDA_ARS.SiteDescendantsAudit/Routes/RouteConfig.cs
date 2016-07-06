using System.Web.Mvc;
using System.Web.Routing;

namespace USDA_ARS.SiteDescendantsAudit.Routes
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            const string pluginBasePath = "App_Plugins/SiteDescendantsAudit";

            routes.MapRoute(
                name: "SiteDescendantsAudit.GetResource",
                url: pluginBasePath + "/{resource}",
                defaults: new
                {
                    controller = "SiteDescendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "SiteDescendantsAudit.GetControllerResource",
                url: pluginBasePath + "/controllers/{resource}",
                defaults: new
                {
                    controller = "SiteDescendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "SiteDescendantsAudit.GetViewResource",
                url: pluginBasePath + "/views/{resource}",
                defaults: new
                {
                    controller = "SiteDescendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "SiteDescendantsAudit.GetVendorResource",
                url: pluginBasePath + "/vendors/{resource}",
                defaults: new
                {
                    controller = "SiteDescendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );
        }
    }
}
