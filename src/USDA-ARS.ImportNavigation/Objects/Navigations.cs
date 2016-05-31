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

            string sql = "SELECT * FROM Navigation WHERE NavSys_ID = @navSysId AND navigation.Parent_NavID = 0";

            List<Navigation> itemList = db.Query<Navigation>(sql, new { navSysId = navSysId }).ToList();

            return itemList;
        }


        public static List<Navigation> GetNavigationListByNavId(int navId)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = "SELECT * FROM Navigation WHERE NavID = @navId";

            List<Navigation> itemList = db.Query<Navigation>(sql, new { navId = navId }).ToList();

            return itemList;
        }
    }
}
