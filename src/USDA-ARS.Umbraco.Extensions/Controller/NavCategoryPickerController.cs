using Archetype.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class NavCategoryPickerController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string Get(string id)
        {
            string output = "";

            try
            {
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

                        output = JsonConvert.SerializeObject(selectList);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda Nav Category Picker Error", ex);
            }

            return output;
        }

    }




}
