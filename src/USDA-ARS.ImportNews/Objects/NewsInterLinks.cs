using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using USDA_ARS.ImportNews.Models;
using USDA_ARS.Umbraco.Extensions.Helpers;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.ImportNews.Objects
{
   public class NewsInterLinks
   {
      public static List<NewsInterLink> GenerateInterLinks(int umbracoNodeId, Guid umbracoNodeGuid, List<LinkItem> linkList, List<ModeCodeLookup> modeCodeList)
      {
         List<NewsInterLink> interLinkList = new List<NewsInterLink>();

         if (linkList != null && linkList.Any())
         {
            foreach (LinkItem linkItem in linkList)
            {
               NewsInterLink interLinkItem = new NewsInterLink();

               interLinkItem.Id = Guid.Empty;

               interLinkItem.UmbracoNodeId = umbracoNodeId;
               interLinkItem.UmbracoNodeGuid = umbracoNodeGuid;

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
                        ModeCodeLookup node = modeCodeList.Where(p => p.UmbracoId == nodeId).FirstOrDefault();

                        if (node != null)
                        {
                           string modeCode = node.ModeCode;

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

                     ModeCodeLookup node = modeCodeList.Where(p => p.Url.ToLower() == url.ToLower()).FirstOrDefault();

                     if (node != null)
                     {
                        string modeCode = node.ModeCode;

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
   }
}
