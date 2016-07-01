using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

using USDA_ARS.LocationsWebApp.DL;
using USDA_ARS.LocationsWebApp.Models;
using USDA_ARS.Umbraco.Extensions.Models.Import;




namespace USDA_ARS.ImportNP
{
    class Program
    {
        static string LOG_FILE_TEXT = "";

         
        
           
        static void Main(string[] args)
        {
            bool forceCacheUpdate = false;

            if (args != null && args.Length == 1)
            {
                forceCacheUpdate = true;
            }

            AddLog("Getting National Programs into Umbraco...");


            ImportNationalPrograms();



        }

       public  static void ImportNationalPrograms()
        {
            AddLog("Getting National Programs into Umbraco...");
            NationalPrograms.ImportNationPrograms();
            AddLog("Succesfully National Programs imported into Umbraco...");

        }
        static void AddLog(string line)
    {
        Debug.WriteLine(line);
        Console.WriteLine(line);
        LOG_FILE_TEXT += line + "\r\n";
    }

}
}
