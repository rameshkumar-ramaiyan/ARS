using System;
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
        public static List<UsdaArsNews> GetNews(int newsCount, string modeCode = null, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            if (dateStart == null)
            {
                dateStart = DateTime.MinValue;
            }
            if (dateEnd == null)
            {
                dateEnd = DateTime.MaxValue;
            }

            Sql sql = null;

            if (true == string.IsNullOrEmpty(modeCode))
            {
                sql = new Sql()
                 .Select("*")
                 .From("UsdaArsNews")
                 .Where("SPSysEndTime is null AND published = 'p' AND originsite_id = 01040000 AND FromField <> 'ARS-Careers <Careers@@ARS.USDA.GOV>' AND NOT SubjectField LIKE 'ARS Newslink%'  AND NOT SubjectField LIKE 'AgResearch Magazine%'")
                 .OrderByDescending("DateField");
            }
            else
            {
                sql = new Sql()
                 .Select("*")
                 .From("UsdaArsNews")
                 .Where("published = 'p' AND FromField <> 'ARS-Careers <Careers@@ARS.USDA.GOV>' AND NOT SubjectField LIKE 'ARS Newslink%'  AND NOT SubjectField LIKE 'AgResearch Magazine%' AND OriginSite_ID LIKE '" + modeCode.Substring(0, 1) + "%'")
                 .OrderByDescending("DateField");
            }

            List<UsdaArsNews> newsItems = db.Query<UsdaArsNews>(sql).Where(p => p.DateField >= dateStart && p.DateField <= dateEnd).Take(newsCount).ToList();

            if (newsItems != null && newsItems.Count > 0)
            {
                newsItems = newsItems.OrderByDescending(p => p.DateField).ToList();
            }

            return newsItems;
        }


        public static UsdaArsNews GetNewsById(int newsId)
        {
            UsdaArsNews newsItem = null;

            var db = ApplicationContext.Current.DatabaseContext.Database;

            Sql sql = null;

            sql = new Sql()
                 .Select("*")
                 .From("UsdaArsNews")
                 .Where("NewsID = " + newsId);

            newsItem = db.Query<UsdaArsNews>(sql).FirstOrDefault();

            return newsItem;
        }
    }
}
