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

            string sql = @"SELECT DISTINCT(OriginSite_ID), OriginSite_Type FROM NavSystems WHERE Published = 'p'
                          --AND OriginSite_Type = 'Place'
                          AND spsysendtime IS NULL
                          --AND SPSysBeginTime > '1/1/1990'
                          --AND showLabel IN(0, 1)
                          AND OriginSite_ID <> '00000000'
                          AND OriginSite_ID	<> ''
                          ORDER BY OriginSite_ID";

            List<NavSystem> itemList = db.Query<NavSystem>(sql).ToList();

            return itemList;
        }

        public static List<NavSystem> GetNavSysListByPlace(string originSite, string type = "Place")
        {
            var db = new Database("sitePublisherDbDSN");

            string sql = @"SELECT * FROM NavSystems WHERE Published = 'p'
                          AND OriginSite_Type = @type
                          AND spsysendtime IS NULL
                          --AND SPSysBeginTime > '1/1/1990'
                          --AND navPageLoc = 'left'
                          --AND showLabel IN(0, 1)
                          AND OriginSite_ID	 <> ''
                          AND OriginSite_ID = @originSite
                          ORDER BY BBSect, NavSysLabel";

            List<NavSystem> itemList = db.Query<NavSystem>(sql, new { type = type, originSite = originSite }).ToList();

            return itemList;
        }


        public static List<NavSystem> GetNavSysListByIndex(string originSite, string type = "Place")
        {   
            var db = new Database("sitePublisherDbDSN");

            string sql = @"SELECT * FROM NavSystems  WHERE Published = 'p'
                          AND OriginSite_Type = @type
                          AND spsysendtime IS NULL
                          --AND navPageLoc = 'left'
                          --AND showLabel IN(0, 1)
                          AND OriginSite_ID = @originSite
                          ORDER BY BBSect, NavSysLabel";

            List<NavSystem> itemList = db.Query<NavSystem>(sql, new { type = type, originSite = originSite }).ToList();

            return itemList;
        }
    }
}
