using USDA_ARS.Umbraco.FileSystemPicker.Utilities;
using System.IO;
using System.Web;
using System.Xml;

namespace USDA_ARS.Umbraco.FileSystemPicker.Install
{
    public static class PluginInstaller
    {
        #region Constants

        private const string APP_VERSION_KEY = "FileSystemPicker:Version";
        private const string APP_VERSION_VALUE = "1.3";

        #endregion

        #region Private Variables

        #endregion

        #region Public Methods

        public static void InstallAppSettingKeys()
        {
            if (PluginHelpers.ReadSetting(APP_VERSION_KEY).Equals("Not Found"))
            {
                PluginHelpers.AddUpdateAppSettings(APP_VERSION_KEY, "Install");
            }
        }

        public static void InstallFiles()
        {
            if (!PluginHelpers.ReadSetting(APP_VERSION_KEY).Equals(APP_VERSION_VALUE))
            {
                InstallPluginFiles();
                PluginHelpers.AddUpdateAppSettings(APP_VERSION_KEY, APP_VERSION_VALUE);
            }
        }
        
        #endregion

        #region Private Methods

        private static void InstallPluginFiles()
        {
            var path = HttpContext.Current.Server.MapPath(@"~\App_Plugins\FileSystemPicker");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"USDA_ARS.Umbraco.FileSystemPicker.Resources", @"App_Plugins");

            path = HttpContext.Current.Server.MapPath(@"~\umbraco\lib\tinymce\plugins\fsmediapicker");
            if (Directory.Exists(path))
            {                
                Directory.Delete(path, true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"USDA_ARS.Umbraco.FileSystemPicker.Resources", @"umbraco");
        }

        #endregion

    }
}
