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
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using USDA_ARS.Core;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Controller
{
    [PluginController("Usda")]
    public class TemplateListController : UmbracoApiController
    {
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]

        public string Get(string id)
        {
            string output = "";

            try
            {
                List<TemplateCms> templateList = null;
                List<TemplateSelectItem> selectList = new List<TemplateSelectItem>();

                var db = new Database("umbracoDbDSN");

                string sql = @"SELECT nodeId, alias FROM cmsTemplate";

                templateList = db.Query<TemplateCms>(sql).ToList();

                selectList.Add(new TemplateSelectItem("", " - "));

                if (templateList != null && templateList.Any())
                {
                    foreach (TemplateCms template in templateList)
                    {
                        selectList.Add(new TemplateSelectItem(template.NodeId.ToString(), template.Alias));
                    }
                }

                output = JsonConvert.SerializeObject(selectList);
            }
            catch (Exception ex)
            {
                LogHelper.Error<DataImporterController>("Usda Template List Request Error", ex);
            }

            return output;
        }
    }

    public class TemplateSelectItem
    {
        public string Id { get; set; }
        public string Template { get; set; }

        public TemplateSelectItem(string id, string template)
        {
            this.Id = id;
            this.Template = template;
        }
    }


    public class TemplateCms
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Alias { get; set; }
    }
}
