using Umbraco.Core;
using USDA_ARS.SiteDescendantsAudit.Install;

namespace USDA_ARS.SiteDescendantsAudit.Events
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();

            base.ApplicationStarted(umbracoApplication, applicationContext);
        }
    }
}
