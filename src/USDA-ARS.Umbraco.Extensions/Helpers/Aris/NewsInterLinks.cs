using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class NewsInterLinks
    {
        public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);


        public static List<NewsInterLink> GetLinksByNewsArticle(int nodeId)
        {
            List<NewsInterLink> linkList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = "SELECT * FROM NewsInterLinks WHERE UmbracoNodeId = @nodeId ORDER BY LinkType";

            linkList = db.Query<NewsInterLink>(sql, new { nodeId = nodeId }).ToList();

            return linkList;
        }


        public static List<IPublishedContent> GetNewsByModeCode(string modeCode)
        {
            List<IPublishedContent> newsList = null;
            List<NewsInterLink> linkList = null;

            modeCode = Helpers.ModeCodes.ModeCodeNoDashes(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            string sql = "SELECT * FROM NewsInterLinks WHERE LinkType = 'place' AND LinkId = @modeCode";

            linkList = db.Query<NewsInterLink>(sql, new { modeCode = modeCode }).ToList();

            if (linkList != null && linkList.Any())
            {
                newsList = new List<IPublishedContent>();

                foreach (NewsInterLink linkItem in linkList)
                {
                    IPublishedContent newsItem = UmbHelper.TypedContent(linkItem.UmbracoNodeId);

                    if (newsItem != null)
                    {
                        newsList.Add(newsItem);
                    }
                }
            }

            return newsList;
        }


        public static List<IPublishedContent> GetNewsByPersonId(long personId)
        {
            List<IPublishedContent> newsList = null;
            List<NewsInterLink> linkList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = "SELECT * FROM NewsInterLinks WHERE LinkType = 'person' AND LinkId = @personId";

            linkList = db.Query<NewsInterLink>(sql, new { personId = personId }).ToList();

            if (linkList != null && linkList.Any())
            {
                newsList = new List<IPublishedContent>();

                foreach (NewsInterLink linkItem in linkList)
                {
                    IPublishedContent newsItem = UmbHelper.TypedContent(linkItem.UmbracoNodeId);

                    if (newsItem != null)
                    {
                        newsList.Add(newsItem);
                    }
                }
            }

            return newsList;
        }


        public static List<NewsInterLink> GenerateInterLinks(IContent content, List<LinkItem> linkList)
        {
            List<NewsInterLink> interLinkList = new List<NewsInterLink>();

            if (linkList != null && linkList.Any())
            {
                foreach (LinkItem linkItem in linkList)
                {
                    NewsInterLink interLinkItem = new NewsInterLink();

                    interLinkItem.Id = Guid.Empty;
                    interLinkItem.UmbracoNodeId = content.Id;
                    interLinkItem.UmbracoNodeGuid = content.Key;

                    if (linkItem.Href != null)
                    {
                        if (linkItem.Href.IndexOf("?person-id") >= 0)
                        {
                            int personId = 0;

                            Match m2 = Regex.Match(linkItem.Href, @"\?person\-id\=(.*)", RegexOptions.Singleline);
                            if (m2.Success)
                            {
                                int.TryParse(m2.Groups[1].Value, out personId);
                            }

                            if (personId > 0)
                            {
                                interLinkItem.LinkType = "person";
                                interLinkItem.LinkId = personId;
                            }
                        }
                        else if (linkItem.Href.IndexOf("/{localLink:") >= 0)
                        {
                            //{localLink:2188}
                            int nodeId = 0;

                            Match m2 = Regex.Match(linkItem.Href, @"{localLink:([\d]*)}", RegexOptions.Singleline);
                            if (m2.Success)
                            {
                                int.TryParse(m2.Groups[1].Value, out nodeId);
                            }

                            if (nodeId > 0)
                            {
                                IPublishedContent node = UmbHelper.TypedContent(nodeId);

                                if (node != null)
                                {
                                    string modeCode = node.GetPropertyValue<string>("modeCode");

                                    if (false == string.IsNullOrEmpty(modeCode))
                                    {
                                        modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

                                        interLinkItem.LinkType = "place";
                                        interLinkItem.LinkId = Convert.ToInt64(modeCode);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string url = linkItem.Href.Replace("http://www.ars.usda.gov", "");

                            IPublishedContent node = Helpers.Nodes.GetNodeByUrl(url);

                            if (node != null)
                            {
                                string modeCode = node.GetPropertyValue<string>("modeCode");

                                if (false == string.IsNullOrEmpty(modeCode))
                                {
                                    modeCode = ModeCodes.ModeCodeNoDashes(modeCode);

                                    interLinkItem.LinkType = "place";
                                    interLinkItem.LinkId = Convert.ToInt64(modeCode);
                                }
                            }
                        }
                    }

                    if (false == string.IsNullOrEmpty(interLinkItem.LinkType))
                    {
                        AddLink(interLinkItem);

                        interLinkList.Add(interLinkItem);
                    }
                }
            }

            return interLinkList;
        }


        public static List<LinkItem> FindInterLinks(string text)
        {
            List<LinkItem> list = new List<LinkItem>();

            // 1.
            // Find all matches in file.
            MatchCollection m1 = Regex.Matches(text, @"(<a.*?>.*?</a>)",
                RegexOptions.Singleline);

            // 2.
            // Loop over each match.
            foreach (Match m in m1)
            {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                RegexOptions.Singleline);
                if (m2.Success)
                {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }
            return list;
        }


        public static NewsInterLink AddLink(NewsInterLink linkItem)
        {
            var db = new Database("arisPublicWebDbDSN");

            if (linkItem != null)
            {
                if (linkItem.Id == Guid.Empty)
                {
                    linkItem.Id = Guid.NewGuid();

                    db.Insert(linkItem);
                }
                else
                {
                    db.Update(linkItem);
                }
            }

            return linkItem;
        }


        public static void RemoveLinksByNodeId(int nodeId)
        {
            var db = new Database("arisPublicWebDbDSN");

            string sql = "DELETE FROM NewsInterLinks WHERE UmbracoNodeId = @nodeId";

            db.Execute(sql, new { nodeId = nodeId });
        }


    }


    public struct LinkItem
    {
        public string Href;
        public string Text;

        public override string ToString()
        {
            return Href + "\n\t" + Text;
        }
    }
}
