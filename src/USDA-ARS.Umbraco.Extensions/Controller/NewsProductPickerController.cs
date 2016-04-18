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
    public class NewsProductPickerController : UmbracoApiController
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
                List<NewsProductItem> selectList = new List<NewsProductItem>();

                if (node != null)
                {
                    if (node.HasValue("newsProducts"))
                    {
                        ArchetypeModel newsProducts = node.GetPropertyValue<ArchetypeModel>("newsProducts");

                        if (newsProducts != null && newsProducts.Any())
                        {
                            selectList.Add(new NewsProductItem { Value = "", Text = "" });

                            foreach (var newsProduct in newsProducts)
                            {
                                string newsProductTitle = newsProduct.GetValue<string>("newsProduct");

                                NewsProductItem newsProductItem = new NewsProductItem();

                                newsProductItem.Value = newsProduct.Id.ToString();
                                newsProductItem.Text = newsProductTitle;

                                selectList.Add(newsProductItem);
                            }

                        }

                        output = JsonConvert.SerializeObject(selectList);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda News Product Picker Error", ex);
            }

            return output;
        }


        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string GetTitle(string id)
        {
            string output = "";

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            IPublishedContent node = Helpers.Nodes.SiteSettings();
            List<NewsProductItem> selectList = new List<NewsProductItem>();

            if (node != null)
            {
                if (node.HasValue("newsProducts"))
                {
                    ArchetypeModel newsProducts = node.GetPropertyValue<ArchetypeModel>("newsProducts");

                    var newsProductItem = newsProducts.Where(p => p.Id.ToString().ToLower() == id.ToLower()).FirstOrDefault();

                    if (newsProductItem != null)
                    {
                        output = newsProductItem.GetValue<string>("");
                    }
                }
            }


                    //UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

                    //IEnumerable<IPublishedContent> nodeList = umbracoHelper.TypedContentAtRoot();

                    //foreach (IPublishedContent node in nodeList)
                    //{
                    //    List<ProductPickerItem> pickerListParent = GetProductList(node);

                    //    ProductPickerItem pickerItemParent = pickerListParent.Where(p => p.Value.ToLower() == id.ToLower()).FirstOrDefault();

                    //    if (pickerItemParent != null)
                    //    {
                    //        return pickerItemParent.Text;
                    //    }

                    //    foreach (IPublishedContent subNode in node.Descendants())
                    //    {
                    //        List<ProductPickerItem> pickerList = GetProductList(subNode);

                    //        ProductPickerItem pickerItem = pickerList.Where(p => p.Value.ToLower() == id.ToLower()).FirstOrDefault();

                    //        if (pickerItem != null)
                    //        {
                    //            return pickerItem.Text;
                    //        }
                    //    }
                    //}

                    return output;
        }

    }




}
