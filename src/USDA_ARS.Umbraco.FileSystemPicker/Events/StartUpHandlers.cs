using USDA_ARS.Umbraco.FileSystemPicker.Install;
using Umbraco.Core;

namespace USDA_ARS.Umbraco.FileSystemPicker.Events
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();
        }
    }
}
