using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Helpers;
using Umbraco.Core.Persistence;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class Nodes
   {
      public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);

      public static IPublishedContent Homepage()
      {
         return UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("Homepage"));
      }

      public static IPublishedContent SiteSettings()
      {
         return UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings"));
      }

      public static IPublishedContent NationProgramsMain()
      {
         return UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("NationalProgramMain"));
      }

      public static IPublishedContent NationProgramsPage()
      {
         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            return homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("NationalProgramsPage"));
         }

         return null;
      }

      public static IEnumerable<IPublishedContent> NewsYearList()
      {
         IPublishedContent newsFolder = Homepage().Descendants().FirstOrDefault(n => n.IsDocumentType("NewsFolder"));

         IEnumerable<IPublishedContent> newsYearList = newsFolder.Children;

         return newsYearList;
      }

      public static IEnumerable<IPublishedContent> MainNavigationList()
      {
         IPublishedContent mainNav = SiteSettings().Children.FirstOrDefault(n => n.IsDocumentType("MainNavigation"));

         return mainNav.Children;
      }

      public static IEnumerable<IPublishedContent> MainOfficeList()
      {
         IPublishedContent mainList = SiteSettings().Children.FirstOrDefault(n => n.IsDocumentType("MainOfficeList"));

         return mainList.Children;
      }

      public static IEnumerable<IPublishedContent> RegionSiteList()
      {
         IPublishedContent ArsLocations = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("ARSLocations"));

         return ArsLocations.Children.Where(n => n.IsDocumentType("Area"));
      }

      public static IEnumerable<IPublishedContent> AllLocationsList()
      {
         IPublishedContent ArsLocations = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("ARSLocations"));

         return ArsLocations.Descendants();
      }

      public static IEnumerable<IPublishedContent> NationalProgramsList()
      {
         IPublishedContent NationalProgramsRoot = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("NationalProgramMain"));

         if (NationalProgramsRoot != null)
         {
            IEnumerable<IPublishedContent> nationalProgramsGroupList = NationalProgramsRoot.Children;

            nationalProgramsGroupList = nationalProgramsGroupList.Where(p => p.DocumentTypeAlias != "SiteNavFolder");

            return nationalProgramsGroupList;
         }

         return null;
      }


      public static IPublishedContent DownloadFile()
      {
         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            return homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("DownloadFile"));
         }

         return null;
      }


      public static IPublishedContent FindALocation()
      {
         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            return homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("FindALocation"));
         }

         return null;
      }

      public static IPublishedContent Careers()
      {
         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            return homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("Careers"));
         }

         return null;
      }

      public static IPublishedContent ProgramsAndProjects()
      {
         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            return homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("ProgramsAndProjects"));
         }

         return null;
      }


      public static IPublishedContent GetEmailTemplate(string nodeName)
      {
         IPublishedContent siteSettings = SiteSettings();

         if (siteSettings != null)
         {
            return siteSettings.Descendants().FirstOrDefault(n => n.IsDocumentType("EmailTemplate") && n.Name.IndexOf(nodeName) >= 0);
         }

         return null;
      }


      public static IEnumerable<IPublishedContent> RegionCityList(IPublishedContent region)
      {
         return region.Children.Where(n => n.IsDocumentType("City"));
      }

      public static void CityResearchUnitList(ref List<ResearchUnitDisplay> researchUnits, IPublishedContent city, int level)
      {
         IEnumerable<IPublishedContent> researchUnitList = city.Children.Where(n => n.IsDocumentType("City") || n.IsDocumentType("ResearchUnit"));

         foreach (IPublishedContent unit in researchUnitList)
         {
            ResearchUnitDisplay researchUnitDisplay = new ResearchUnitDisplay();

            researchUnitDisplay.ResearchUnit = unit;
            researchUnitDisplay.Level = level;

            researchUnits.Add(researchUnitDisplay);

            CityResearchUnitList(ref researchUnits, unit, level + 1);
         }
      }

      public static List<List<ResearchUnitDisplay>> ResearchUnitsSplit(List<ResearchUnitDisplay> researchUnits)
      {
         List<List<ResearchUnitDisplay>> unitsSplits = new List<List<ResearchUnitDisplay>>();
         List<ResearchUnitDisplay> unitsOneList = new List<ResearchUnitDisplay>();
         List<ResearchUnitDisplay> unitsTwoList = new List<ResearchUnitDisplay>();

         int i = 0;
         int mid = researchUnits.Count / 2;

         foreach (ResearchUnitDisplay unit in researchUnits)
         {
            if (i < mid)
            {
               unitsOneList.Add(unit);
            }
            else
            {
               unitsTwoList.Add(unit);
            }

            i++;
         }

         unitsSplits.Add(unitsOneList);
         unitsSplits.Add(unitsTwoList);

         return unitsSplits;
      }


      public static List<IPublishedContent> NewsList()
      {
         List<IPublishedContent> newsList = new List<IPublishedContent>();

         IPublishedContent homepage = Homepage();

         if (homepage != null)
         {
            IPublishedContent newsContainer = homepage.Descendants().FirstOrDefault(n => n.IsDocumentType("News"));

            if (newsContainer != null)
            {
               newsList = newsContainer.Descendants().Where(p => p.IsDocumentType("NewsArticle")).ToList();
            }
         }

         return newsList;
      }


      public static List<RedirectItem> NodesWithRedirectsList(bool useCache = true)
      {
         List<RedirectItem> redirectList = null;

         string cacheKey = "RedirectList";
         ObjectCache cache = MemoryCache.Default;

         if (true == useCache)
         {
            redirectList = cache.Get(cacheKey) as List<RedirectItem>;
         }

         if (redirectList == null)
         {
            redirectList = new List<RedirectItem>();

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
               if (root != null)
               {
                  if (false == string.IsNullOrWhiteSpace(root.GetPropertyValue<string>("oldUrl")))
                  {
                     List<string> oldUrlArray = root.GetPropertyValue<string>("oldUrl").Split(',').ToList();

                     if (oldUrlArray != null && oldUrlArray.Count > 0)
                     {
                        foreach (string oldUrl in oldUrlArray)
                        {
                           redirectList.Add(new RedirectItem { OldUrl = oldUrl.Trim().ToLower(), UmbracoId = root.Id });
                        }
                     }
                  }

                  foreach (IPublishedContent subNode in root.Descendants().Where(n => false == string.IsNullOrWhiteSpace(n.GetPropertyValue<string>("oldUrl"))))
                  {
                     List<string> oldUrlArray = subNode.GetPropertyValue<string>("oldUrl").Split(',').ToList();

                     if (oldUrlArray != null && oldUrlArray.Count > 0)
                     {
                        foreach (string oldUrl in oldUrlArray)
                        {
                           redirectList.Add(new RedirectItem { OldUrl = oldUrl.Trim().ToLower(), UmbracoId = subNode.Id });
                        }
                     }
                  }
               }
            }

            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(240) };
            cache.Add(cacheKey, redirectList, policy);
         }

         return redirectList;
      }


      public static List<string> StateListFromRegion(IPublishedContent region)
      {
         List<string> stateList = new List<string>();

         IEnumerable<IPublishedContent> cityList = region.Children.Where(n => n.IsDocumentType("City"));

         foreach (IPublishedContent city in cityList)
         {
            stateList.Add(city.GetPropertyValue<string>("state"));
         }

         if (stateList != null && stateList.Count > 0)
         {
            stateList = stateList.Distinct().ToList();
         }

         return stateList;
      }


      public static IPublishedContent GetNodeByModeCode(string modeCode, bool useCache = true)
      {
         IPublishedContent node = null;
         modeCode = Helpers.ModeCodes.ModeCodeAddDashes(modeCode);

         if (true == useCache)
         {
            List<IPublishedContent> nodeList = GetNodesListOfModeCodes();

            node = nodeList.Where(p => p.GetPropertyValue<string>("modeCode") == modeCode).FirstOrDefault();
         }
         else
         {
            var db = new Database("umbracoDbDSN");

            string sql = @"SELECT contentNodeId FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'modeCode')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar = @modeCode AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

            string contentNodeId = db.Query<string>(sql, new { modeCode = modeCode }).FirstOrDefault();

            if (false == string.IsNullOrEmpty(contentNodeId))
            {
               node = UmbHelper.TypedContent(Convert.ToInt32(contentNodeId));
            }
         }


         return node;
      }


      public static List<IPublishedContent> GetNodesListOfModeCodes(bool useCache = true)
      {
         List<IPublishedContent> nodeList = null;
         string cacheKey = "NodeListByModeCodes";
         int cacheUpdateIntMinutes = 1440;

         ObjectCache cache = MemoryCache.Default;

         if (true == useCache)
         {
            nodeList = cache.Get(cacheKey) as List<IPublishedContent>;
         }

         if (nodeList == null)
         {
            nodeList = new List<IPublishedContent>();

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
               if (false == string.IsNullOrEmpty(root.GetPropertyValue<string>("modeCode")))
               {
                  nodeList.Add(root);
               }
               else
               {
                  nodeList.AddRange(root.Descendants().Where(n => false == string.IsNullOrEmpty(n.GetPropertyValue<string>("modeCode"))));
               }
            }

            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateIntMinutes) };
            cache.Add(cacheKey, nodeList, policy);
         }


         return nodeList;
      }


      public static IPublishedContent GetNodeByUrl(string url)
      {
         IPublishedContent node = null;

         foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
         {
            if (node == null)
            {
               node = root.Descendants().FirstOrDefault(n => n.Url.ToLower() == url.ToLower());
            }
         }

         return node;
      }


      public static IPublishedContent GetNodeByNpCode(string npCode)
      {
         IPublishedContent node = null;

         foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
         {
            if (node == null)
            {
               node = root.Descendants().FirstOrDefault(n => n.GetPropertyValue<string>("npCode") == npCode);
            }
         }

         return node;
      }


      public static IPublishedContent GetNodeByPersonId(int personId)
      {
         IPublishedContent node = null;

         foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
         {
            if (node == null)
            {
               node = root.Descendants().Where(p => p.DocumentTypeAlias == "PersonSite").FirstOrDefault(n => n.GetPropertyValue<string>("personLink") == personId.ToString());
            }
         }

         return node;
      }


      public static IPublishedContent GetNodeById(int nodeId)
      {
         IPublishedContent node = null;

         node = UmbHelper.TypedContent(nodeId);

         return node;
      }


      public static IPublishedContent GetNodeByOldDocId(string docId)
      {
         IPublishedContent node = null;

         var db = new Database("umbracoDbDSN");

         string sql = @"SELECT * FROM cmsPropertyData WHERE propertytypeid IN (SELECT id FROM cmsPropertyType WHERE Alias = 'oldId')
                            AND NOT dataNvarchar IS NULL AND dataNvarchar = @docId AND versionId IN
                            (SELECT versionId FROM cmsDocument WHERE published = 1)";

         UmbracoPropertyData propertyData = db.Query<UmbracoPropertyData>(sql, new { docId = docId }).FirstOrDefault();

         if (propertyData != null)
         {
            node = UmbHelper.TypedContent(Convert.ToInt32(propertyData.UmbracoId));
         }

         return node;
      }
   }
}
