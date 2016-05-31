using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNavigation.Models;

namespace USDA_ARS.ImportNavigation.Objects
{
    public class SubSites
    {
        public static List<SubSite> GetValidSiteList()
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM Sites WHERE SPSysEndTime IS NULL AND site_status = 1";

            List<SubSite> siteList = db.Query<SubSite>(sql).ToList();

            return siteList;
        }
    }
}
