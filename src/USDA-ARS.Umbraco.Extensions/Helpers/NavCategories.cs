using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class NavCategories
    {
        public static string GetNavGuidByName(string name)
        {
            string output = "";

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            IPublishedContent node = Helpers.Nodes.SiteSettings();
            List<NavCategoryItem> selectList = new List<NavCategoryItem>();

            if (node != null)
            {
                if (node.HasValue("navigationCategories"))
                {
                    ArchetypeModel navigationCategories = node.GetPropertyValue<ArchetypeModel>("navigationCategories");

                    if (navigationCategories != null && navigationCategories.Any())
                    {
                        foreach (var navCategory in navigationCategories)
                        {
                            string categoryTitle = navCategory.GetValue<string>("categoryTitle");

                            NavCategoryItem navCategoryItem = new NavCategoryItem();

                            navCategoryItem.Value = navCategory.Id.ToString();
                            navCategoryItem.Text = categoryTitle;

                            selectList.Add(navCategoryItem);
                        }

                    }

                    NavCategoryItem itemFound = selectList.Where(p => p.Text == name).FirstOrDefault();

                    if (itemFound != null)
                    {
                        output = itemFound.Value.ToLower();
                    }
                }
            }

            return output;
        }
    }
}
