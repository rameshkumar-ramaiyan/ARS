using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using USDA_ARS.LocationsWebApp.Models;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class PopularTopics
    {
        public static List<PopularLink> GetPopularTopicsByModeCode(string modeCode)
        {
            modeCode = Umbraco.Extensions.Helpers.ModeCodes.ModeCodeNoDashes(modeCode);

            var db = new Database("redesignDbDSN");

            string sql = @"select Label, URL
			                    from	PopularLink
			                    where	modecode = @modeCode
			                    order by SortOrder";

            List<PopularLink> linkList = db.Query<PopularLink>(sql, new { modeCode = modeCode }).ToList();

            return linkList;
        }
    }
}