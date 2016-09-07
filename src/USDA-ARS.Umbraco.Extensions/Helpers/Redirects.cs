using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class Redirects
   {
      private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

      public static string RedirectUrl(string badUrl)
      {
         string redirectUrl = null;

         if (false == string.IsNullOrEmpty(badUrl))
         {
            if (false == badUrl.ToLower().StartsWith("/umbraco/"))
            {
               // Magazine
               redirectUrl = RedirectMagazineUrl(badUrl);

               // Redirect Pages
               if (true == string.IsNullOrEmpty(redirectUrl))
               {
                  redirectUrl = RedirectOldPages(badUrl);
               }

               // Redirects In Umbraco
               if (true == string.IsNullOrEmpty(redirectUrl))
               {
                  redirectUrl = RedirectPageStoredInUmbraco(badUrl);
               }

               // Redirects Iwith Doc ID
               if (true == string.IsNullOrEmpty(redirectUrl))
               {
                  redirectUrl = RedirectWithDocId(badUrl);
               }
            }
         }


         return redirectUrl;
      }


      /// <summary>
      /// Redirect ARS Magazine URLs
      /// </summary>
      /// <param name="badUrl"></param>
      /// <returns></returns>
      public static string RedirectMagazineUrl(string badUrl)
      {
         string redirectUrl = null;

         // Is this a AgMagazine URL?
         if (badUrl.ToLower().IndexOf("/is/ar") >= 0)
         {
            // /is/AR/2016/mar16/cookies.htm

            List<string> badUrlArray = badUrl.ToLower().Split('/').ToList();

            if (badUrlArray != null && badUrlArray.Count == 6)
            {
               string agMagUrl = "https://agresearchmag.ars.usda.gov/";

               int index = 4;

               string month = badUrlArray[index].Substring(0, 3);
               string year = badUrlArray[index].Substring(3, 2);

               if (Convert.ToInt32(year) <= 99 && Convert.ToInt32(year) >= 50)
               {
                  year = "19" + year;
               }
               else
               {
                  year = "20" + year;
               }

               agMagUrl += year + "/";
               agMagUrl += month + "/";
               index++;
               string endUrl = badUrlArray[index].Replace(".htm", "");

               if (false == string.IsNullOrEmpty(endUrl) && false == endUrl.ToLower().EndsWith(".pdf"))
               {
                  if (endUrl.Length >= 4)
                  {
                     endUrl = endUrl.Substring(0, endUrl.Length - 4);
                  }
               }

               agMagUrl += endUrl;

               if (endUrl.ToLower().EndsWith(".pdf"))
               {
                  agMagUrl = agMagUrl.Replace("https://agresearchmag.ars.usda.gov/", "https://agresearchmag.ars.usda.gov/ar/archive/");
               }


               redirectUrl = agMagUrl;
            }
         }

         return redirectUrl;
      }


      /// <summary>
      /// Redirects for random/dynamic pages
      /// </summary>
      /// <param name="badUrl"></param>
      /// <returns></returns>
      public static string RedirectOldPages(string badUrl)
      {
         string redirectUrl = null;

         if (badUrl.ToLower().IndexOf("/main/site_main.htm?modecode=") >= 0)
         {
            string modeCodeFind = badUrl.ToLower().Replace("/main/site_main.htm?modecode=", "");
            IPublishedContent findNode = Nodes.GetNodeByModeCode(modeCodeFind);

            if (findNode != null)
            {
               redirectUrl = findNode.Url;
            }
         }

         if (badUrl.ToLower().IndexOf("/research/programs/usmap.htm?stateabbr=") >= 0 && badUrl.ToLower().IndexOf("&np_code=") >= 0)
         {
            // "/research/research-programs-by-state/?state=AR&npCode=107";
            IPublishedContent findNode = Nodes.GetNodeById(9130); // Research Programs by State node

            if (findNode != null)
            {
               Match m2 = Regex.Match(badUrl, @"stateabbr=([^&]*)&np_code=([\d]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
               if (m2.Success)
               {
                  redirectUrl = findNode.Url + "?state=" + m2.Groups[1].Value + "&npCode=" + m2.Groups[2].Value;
               }
            }
         }


         // If it didn't find, look through list of custom redirects
         if (true == string.IsNullOrEmpty(redirectUrl))
         {
            List<RedirectToNode> redirectList = GetListOfRedirects();

            if (redirectList != null && redirectList.Any())
            {
               foreach (RedirectToNode redirectNode in redirectList)
               {
                  if (true == string.IsNullOrEmpty(redirectUrl))
                  {
                     if (badUrl.ToLower().IndexOf(redirectNode.OldUrl.ToLower()) >= 0)
                     {
                        IPublishedContent nodeFound = Nodes.GetNodeById(redirectNode.UmbracoId);

                        if (nodeFound != null)
                        {
                           redirectUrl = badUrl.ToLower().Replace(redirectNode.OldUrl, nodeFound.Url + redirectNode.AppendString);
                        }
                     }
                  }
               }
            }

         }
         
         return redirectUrl;
      }


      public static List<RedirectToNode> GetListOfRedirects()
      {
         List<RedirectToNode> redirectList = new List<RedirectToNode>();

         // MAKE SURE EVERYTHING IS IN LOWER CASE!
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/locations/NPSLocation.htm?modecode=", UmbracoId = 9127, AppendString = "?modeCode=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/locations/locations.htm?modecode=", UmbracoId = 200481, AppendString = "?modeCode=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/people.htm?personid=", UmbracoId = 6992, AppendString = "?person-id=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/research/projects/projects.htm?accn_no=", UmbracoId = 8092, AppendString = "?accnNo=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/services/TTBrowse.htm?stp_code=", UmbracoId = 8101, AppendString = "?stpCode=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/services/software/download.htm?softwareid=", UmbracoId = 8066, AppendString = "?softwareid=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/research/publications/publications.htm?seq_no_115=", UmbracoId = 9114, AppendString = "?seqNo115=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/research/projects_programs.htm?modecode=", UmbracoId = 8089, AppendString = "?modeCode=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/research/programs/programs.htm?projectlist=true&NP_CODE=", UmbracoId = 000, AppendString = "?" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/address.htm?personid=", UmbracoId = 200341, AppendString = "?person-id=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/news.htm?personid=", UmbracoId = 200348, AppendString = "?person-id=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/projects.htm?personid=", UmbracoId = 200349, AppendString = "?person-id=" });
         redirectList.Add(new RedirectToNode() { OldUrl = "/pandp/people/publications.htm?personid=", UmbracoId = 200350, AppendString = "?person-id=" });
         


         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });
         //redirectList.Add(new RedirectToNode() { OldUrl = "", UmbracoId = 000, AppendString = "?" });

         // /research/programs/usmap.htm?stateabbr=OK&NP_CODE=101 
         // /research/research-programs-by-state/?state=AR&npCode=107

         return redirectList;
      }


      /// <summary>
      /// Redirects with Umbraco pages that have a Old URL
      /// </summary>
      /// <param name="badUrl"></param>
      /// <returns></returns>
      public static string RedirectPageStoredInUmbraco(string badUrl)
      {
         string redirectUrl = null;

         List<RedirectItem> nodesList = Nodes.NodesWithRedirectsList();

         if (nodesList != null && nodesList.Any())
         {
            RedirectItem redirectItem = nodesList.Where(p => p.OldUrl == badUrl.ToLower()).FirstOrDefault();

            if (redirectItem != null)
            {
               int umbracoId = redirectItem.UmbracoId;

               IPublishedContent foundRedirectNode = Nodes.GetNodeById(umbracoId);

               if (foundRedirectNode != null)
               {
                  redirectUrl = foundRedirectNode.Url;
               }
            }
         }

         return redirectUrl;
      }


      /// <summary>
      /// Redirects that have a Doc ID in Umbraco
      /// </summary>
      /// <param name="badUrl"></param>
      /// <returns></returns>
      public static string RedirectWithDocId(string badUrl)
      {
         string redirectUrl = null;

         if (badUrl.ToLower().IndexOf("docid=") >= 0)
         {
            int docId = 0;

            Match m2 = Regex.Match(badUrl, @"docid=([\d]*)", RegexOptions.Singleline);
            if (m2.Success)
            {
               if (int.TryParse(m2.Groups[1].Value, out docId))
               {
                  IPublishedContent docNode = Nodes.GetNodeByOldDocId(docId.ToString());

                  if (docNode != null)
                  {
                     redirectUrl = docNode.Url;
                  }
               }
            }
         }

         return redirectUrl;
      }
   }

   public class RedirectToNode
   {
      public string OldUrl { get; set; }
      public int UmbracoId { get; set; }
      public string AppendString { get; set; }
      public string ManualUrl { get; set; }
   }
}
