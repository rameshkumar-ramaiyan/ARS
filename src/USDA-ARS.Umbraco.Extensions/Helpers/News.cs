﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class News
    {
        public static List<UsdaArsNews> GetNews(int newsCount, string modeCode = null)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            Sql sql = null;

            if (true == string.IsNullOrEmpty(modeCode))
            {
                sql = new Sql()
                 .Select("*")
                 .From("UsdaArsNews")
                 .Where("published = 'p'")
                 .OrderByDescending("DateField");
            }
            else
            {
                sql = new Sql()
                 .Select("*")
                 .From("UsdaArsNews")
                 .Where("published = 'p' AND OriginSite_ID LIKE '" + modeCode.Substring(0, 1) + "%'")
                 .OrderByDescending("DateField");
            }

            List<UsdaArsNews> newsItems = db.Query<UsdaArsNews>(sql).Take(newsCount).ToList();

            return newsItems;
        }
    }
}
