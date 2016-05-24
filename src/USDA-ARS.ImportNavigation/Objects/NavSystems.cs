using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNavigation.Models;

namespace USDA_ARS.ImportNavigation.Objects
{
    public class NavSystems
    {
        public static List<NavSystem> GetNavModeCodeList()
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = @"SELECT DISTINCT(OriginSite_ID) FROM NavSystems  WHERE Published = 'p'
                          AND OriginSite_Type = 'Place'
                          AND spsysendtime IS NULL
                          AND showLabel IN(0, 1)
                          AND OriginSite_ID <> '00000000'
                          ORDER BY OriginSite_ID";

            List<NavSystem> itemList = db.Query<NavSystem>(sql).ToList();

            return itemList;
        }

        public static List<NavSystem> GetNavSysListByPlace(string modeCode)
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = @"SELECT * FROM NavSystems  WHERE Published = 'p'
                          AND OriginSite_Type = 'Place'
                          AND spsysendtime IS NULL
                          --AND navPageLoc = 'left'
                          AND showLabel IN(0, 1)
                          AND OriginSite_ID = @modeCode
                          ORDER BY BBSect, NavSysLabel";

            List<NavSystem> itemList = db.Query<NavSystem>(sql, new { modeCode = modeCode }).ToList();

            return itemList;
        }
    }
}
