using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using USDA_ARS.SiteDescendantsAudit.Utilities;

namespace USDA_ARS.SiteDescendantsAudit.Install
{
    public static class PluginInstaller
    {
        #region Constants

        private const string APP_VERSION_KEY = "SiteDescendantsAudit:Version";
        private const string APP_VERSION_VALUE = "1.0";

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
            if (Directory.Exists(@"~\App_Plugins\SiteDescendantsAudit"))
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(@"~\App_Plugins\SiteDescendantsAudit"), true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"USDA_ARS.SiteDescendantsAudit.Resources", @"App_Plugins");
        }

        #endregion

    }

}
