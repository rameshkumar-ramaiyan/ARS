using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class News
    {
        public static List<IPublishedContent> GetNews(int newsCount, string modeCode = null, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            List<IPublishedContent> newsItems = null;

            newsItems = Helpers.Nodes.NewsList();

            if (dateStart != null && dateEnd != null)
            {
                newsItems = newsItems.Where(p => p.GetPropertyValue<DateTime>("articleDate") >= dateStart && p.GetPropertyValue<DateTime>("articleDate") <= dateEnd).ToList();
            }

            newsItems = newsItems.OrderByDescending(p => p.GetPropertyValue<DateTime>("articleDate")).ToList();

            newsItems = newsItems.Take(newsCount).ToList();

            return newsItems;
        }

    }
}
