using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Helpers;

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

            return ArsLocations.Children.Where(n => n.IsDocumentType("Region"));
        }

        public static IEnumerable<IPublishedContent> AllLocationsList()
        {
            IPublishedContent ArsLocations = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("ARSLocations"));

            return ArsLocations.Descendants();
        }

        public static IEnumerable<IPublishedContent> NationalProgramsList()
        {
            IPublishedContent NationalProgramsRoot = UmbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("NationalProgramsRoot"));

            if (NationalProgramsRoot != null)
            {
                return NationalProgramsRoot.Children.FirstOrDefault().Children;
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

            foreach(ResearchUnitDisplay unit in researchUnits)
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


        public static List<IPublishedContent> NodesWithRedirectsList()
        {
            List<IPublishedContent> nodeList = new List<IPublishedContent>();

            IPublishedContent node = null;

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
                if (node == null)
                {
                    nodeList.AddRange(root.Descendants().Where(n => false == string.IsNullOrWhiteSpace(n.GetPropertyValue<string>("oldUrl"))).ToList());
                }
            }

            return nodeList;
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


        public static IPublishedContent GetNodeByModeCode(string modeCode)
        {
            IPublishedContent node = null;
            modeCode = Helpers.ModeCodes.ModeCodeAddDashes(modeCode);

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
                if (node == null)
                {
                    node = root.Descendants().FirstOrDefault(n => n.GetPropertyValue<string>("modeCode") == modeCode);
                }
            }

            return node;
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
    }
}
