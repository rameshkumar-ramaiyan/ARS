using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDA_ARS.ImportPeopleSites.Models;
using USDA_ARS.ImportPeopleSites.Objects;

namespace USDA_ARS.ImportPeopleSites
{
    class Program
    {
        static string LOG_FILE_TEXT = "";

        static string API_KEY = ConfigurationManager.AppSettings.Get("Umbraco:ApiKey");
        static string API_URL = ConfigurationManager.AppSettings.Get("Umbraco:ApiUrl");

        static List<PeopleFolderLookup> PEOPLE_FOLDER_LIST = null;

        static void Main(string[] args)
        {
            bool forceCacheUpdate = false;

            if (args != null && args.Length >= 1)
            {
                if (args[0] == "force-cache-update")
                {
                    forceCacheUpdate = true;
                }
            }

            Logs.AddLog(ref LOG_FILE_TEXT, "Getting People Folders From Umbraco...");
            PeopleFolders.GenerateModeCodeFolderList(ref PEOPLE_FOLDER_LIST, ref LOG_FILE_TEXT, forceCacheUpdate);
            Logs.AddLog(ref LOG_FILE_TEXT, "Done. Count: " + PEOPLE_FOLDER_LIST.Count);
            Logs.AddLog(ref LOG_FILE_TEXT, "");

        }
    }
}
