using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Blogs
    {
        public static string BLOG_FEED_URL = "http://blogs.usda.gov/tag/ars/feed/";
        public static string BLOG_FEED_PATH = HttpContext.Current.Server.MapPath("/App_Data/blog-feed.txt");

        public static List<BlogItem> GetBlogFeed()
        {
            List<BlogItem> blogList = null;
            string cacheKey = "BlogCache";
            ObjectCache cache = MemoryCache.Default;

            blogList = cache.Get(cacheKey) as List<BlogItem>;

            if (blogList == null)
            {
                blogList = new List<BlogItem>();
                XmlReader reader = null;

                try
                {
                    reader = XmlReader.Create(BLOG_FEED_URL);

                    blogList = ProcessBlogFeed(reader);

                    if (blogList.Count > 0)
                    {
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) };
                        cache.Add(cacheKey, blogList, policy);

                        SaveBlogFeed(reader);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Blogs>("Error getting USDA Blog feed", ex);

                    // Try different method
                    reader = LoadSavedBlogFeed(FixRssFeed());

                    // Getting blog list from text file
                    if (reader == null)
                    {
                        reader = LoadSavedBlogFeed();
                    }

                    if (reader != null)
                    {
                        blogList = ProcessBlogFeed(reader);

                        if (blogList.Count > 0)
                        {
                            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) };
                            cache.Add(cacheKey, blogList, policy);

                            SaveBlogFeed(reader);
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            return blogList;
        }


        private static List<BlogItem> ProcessBlogFeed(XmlReader blogFeed)
        {
            List<BlogItem> blogItemList = new List<BlogItem>();
            SyndicationFeed feed = null;

            feed = SyndicationFeed.Load(blogFeed);

            IEnumerable<SyndicationItem> feedList = feed.Items;

            foreach (SyndicationItem item in feedList)
            {
                if (item.Categories.Where(p => p.Name == "ARS").Count() > 0)
                {
                    BlogItem blogItem = new BlogItem();

                    blogItem.Title = item.Title.Text;
                    blogItem.Summary = item.Summary.Text;
                    blogItem.Url = item.Links[0].Uri.AbsoluteUri;

                    blogItemList.Add(blogItem);
                }
            }

            return blogItemList;
        }


        private static void SaveBlogFeed(XmlReader blogFeed)
        {
            try
            {
                using (StreamWriter outputFile = new StreamWriter(BLOG_FEED_PATH))
                {
                    outputFile.WriteLine(blogFeed.ReadOuterXml());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(typeof(Blogs), "ERROR: Failed to save blog file: " + ex.ToString());
            }
        }


        public static XmlReader LoadSavedBlogFeed(string xmlString = null)
        {
            XmlReader reader = null;

            if (true == File.Exists(BLOG_FEED_PATH) || false == string.IsNullOrEmpty(xmlString))
            {
                try
                {
                    if (true == string.IsNullOrEmpty(xmlString))
                    {
                        xmlString = File.ReadAllText(BLOG_FEED_PATH);
                    }

                    xmlString = xmlString.Replace("\x92", "'");
                    xmlString = xmlString.Replace(" & ", " &amp; ");
                    xmlString = xmlString.Replace("A&M", "A&amp;M");

                    reader = XmlReader.Create(new StringReader(xmlString));
                }
                catch (Exception ex)
                {
                    LogHelper.Warn(typeof(Blogs), "ERROR: Failed to load blog file: " + ex.ToString());
                }
            }

            return reader;
        }


        public static string FixRssFeed()
        {
            WebClient webClient = null;
            string rssString = "";

            try
            {
                webClient = new WebClient();
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows; Windows NT 5.1; rv:1.9.2.4) Gecko/20100611 Firefox/3.6.4");

                Stream blogStream = webClient.OpenRead(BLOG_FEED_URL);

                StreamReader sr = new StreamReader(blogStream);
                rssString = sr.ReadToEnd();
                blogStream.Close();
            }
            finally
            {
                if (webClient != null)
                { webClient.Dispose(); }
            }

            return rssString;
        }
    }
}
