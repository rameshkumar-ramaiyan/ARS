﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Xml;
using System.ServiceModel.Syndication;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Blogs
    {
        public static List<BlogItem> GetBlogFeed()
        {
            List<BlogItem> blogList = null;
            string cacheKey = "BlogCache";
            ObjectCache cache = MemoryCache.Default;

            blogList = cache.Get(cacheKey) as List<BlogItem>;

            if (blogList == null)
            {
                blogList = new List<BlogItem>();

                string url = "http://blogs.usda.gov/tag/ars/feed/";
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                reader.Close();

                IEnumerable<SyndicationItem> feedList = feed.Items;

                foreach (SyndicationItem item in feedList)
                {
                    if (item.Categories.Where(p => p.Name == "ARS").Count() > 0)
                    {
                        BlogItem blogItem = new BlogItem();

                        blogItem.Title = item.Title.Text;
                        blogItem.Summary = item.Summary.Text;
                        blogItem.Url = item.Links[0].Uri.AbsoluteUri;

                        blogList.Add(blogItem);
                    }
                }

                if (blogList.Count > 0)
                {
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) };
                    cache.Add(cacheKey, blogList, policy);
                }
            }


            return blogList;
        }
    }
}