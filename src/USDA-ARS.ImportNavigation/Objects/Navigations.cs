using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNavigation.Models;

namespace USDA_ARS.ImportNavigation.Objects
{
    public class Navigations
    {
        public static List<Navigation> GetNavigationList(int navSysId)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM Navigation WHERE NavSys_ID = @navSysId";

            List<Navigation> itemList = db.Query<Navigation>(sql, new { navSysId = navSysId }).ToList();

            return itemList;
        }
    }
}
