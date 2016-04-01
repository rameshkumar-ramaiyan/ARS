using Archetype.Models;
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
using USDA_ARS.Umbraco.Extensions.Models;
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


        public static List<IPublishedContent> GetNewsByProduct(string productName, int newsCount, string modeCode = null, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            List<IPublishedContent> newsItems = null;

            List<NewsProductItem> newsProductList = GetNewsProductList();

            if (newsProductList != null && newsProductList.Count > 0)
            {
                NewsProductItem newsProductFound = newsProductList.Where(p => p.Text == productName).FirstOrDefault();

                if (newsProductFound != null)
                {
                    newsItems = Helpers.Nodes.NewsList();

                    newsItems = newsItems.Where(p => p.GetPropertyValue<string>("newsProductsList") != null &&
                                p.GetPropertyValue<string>("newsProductsList").ToLower() == newsProductFound.Value.ToLower()).ToList();

                    if (dateStart != null && dateEnd != null)
                    {
                        newsItems = newsItems.Where(p => p.GetPropertyValue<string>("newsProductsList") != null &&
                                p.GetPropertyValue<string>("newsProductsList").ToLower() == newsProductFound.Value.ToLower() && 
                                p.GetPropertyValue<DateTime>("articleDate") >= dateStart && p.GetPropertyValue<DateTime>("articleDate") <= dateEnd).ToList();
                    }

                    newsItems = newsItems.OrderByDescending(p => p.GetPropertyValue<DateTime>("articleDate")).ToList();

                    newsItems = newsItems.Take(newsCount).ToList();
                }
            }

            return newsItems;
        }


        public static List<IPublishedContent> GetNewsByTopic(string topicName, int newsCount, string modeCode = null, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            List<IPublishedContent> newsItems = null;

            List<NewsTopicItem> newsTopicList = GetNewsTopicList();

            if (newsTopicList != null && newsTopicList.Count > 0)
            {
                NewsTopicItem newsTopicFound = newsTopicList.Where(p => p.Text == topicName).FirstOrDefault();

                if (newsTopicFound != null)
                {
                    newsItems = Helpers.Nodes.NewsList();

                    newsItems = newsItems.Where(p => p.GetPropertyValue<string>("newsTopicsList") != null).ToList();

                    string newsTopicGuid = newsTopicFound.Value.ToLower();

                    //newsItems = newsItems.Where(p => p.GetPropertyValue<string>("newsTopicsList").ToLower().Contains(newsTopicGuid)).ToList();

                    if (dateStart != null && dateEnd != null)
                    {
                        newsItems = newsItems.Where(p => p.GetPropertyValue<string>("newsTopicList") != null &&
                            p.GetPropertyValue<string>("newsTopicsList").ToLower().Contains(newsTopicFound.Value.ToLower()) &&
                            p.GetPropertyValue<DateTime>("articleDate") >= dateStart && p.GetPropertyValue<DateTime>("articleDate") <= dateEnd).ToList();
                    }

                    newsItems = newsItems.OrderByDescending(p => p.GetPropertyValue<DateTime>("articleDate")).ToList();

                    newsItems = newsItems.Take(newsCount).ToList();
                }
            }

            return newsItems;
        }


        public static List<NewsProductItem> GetNewsProductList()
        {
            List<NewsProductItem> productList = new List<NewsProductItem>();
            IPublishedContent node = Helpers.Nodes.SiteSettings();

            if (node != null)
            {
                if (node.HasValue("newsProducts"))
                {
                    ArchetypeModel newsProducts = node.GetPropertyValue<ArchetypeModel>("newsProducts");

                    if (newsProducts != null && newsProducts.Any())
                    {
                        foreach (var newsProduct in newsProducts)
                        {
                            string newsProductTitle = newsProduct.GetValue<string>("newsProduct");

                            NewsProductItem newsProductItem = new NewsProductItem();

                            newsProductItem.Value = newsProduct.Id.ToString();
                            newsProductItem.Text = newsProductTitle;

                            productList.Add(newsProductItem);
                        }

                    }
                }
            }

            return productList;
        }


        public static List<NewsTopicItem> GetNewsTopicList()
        {
            List<NewsTopicItem> topicList = new List<NewsTopicItem>();
            IPublishedContent node = Helpers.Nodes.SiteSettings();

            if (node != null)
            {
                if (node.HasValue("newsTopics"))
                {
                    ArchetypeModel newsTopics = node.GetPropertyValue<ArchetypeModel>("newsTopics");

                    if (newsTopics != null && newsTopics.Any())
                    {
                        foreach (var newsTopic in newsTopics)
                        {
                            string newsTopicTitle = newsTopic.GetValue<string>("newsTopic");

                            NewsTopicItem newsTopicItem = new NewsTopicItem();

                            newsTopicItem.Value = newsTopic.Id.ToString();
                            newsTopicItem.Text = newsTopicTitle;

                            topicList.Add(newsTopicItem);
                        }

                    }
                }
            }

            return topicList;
        }

    }
}
