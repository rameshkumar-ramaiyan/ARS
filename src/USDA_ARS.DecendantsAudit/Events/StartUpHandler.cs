using Umbraco.Core;
using USDA_ARS.SiteDecendantsAudit.Install;

namespace USDA_ARS.SiteDecendantsAudit.Events
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
