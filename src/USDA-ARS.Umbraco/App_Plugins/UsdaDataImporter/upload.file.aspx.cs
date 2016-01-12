using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.App_Plugins.UsdaDataImporter
{
    public partial class upload_file : System.Web.UI.Page
    {
        public string Id = "";
        public string defaultTableName = "Schedule Table";

        protected void Page_Load(object sender, EventArgs e)
        {
            Id = Request.QueryString.Get("id");
            IPublishedContent siteSettings = USDA_ARS.Umbraco.Extensions.Helpers.Nodes.SiteSettings();

            if (siteSettings != null)
            {
                defaultTableName = siteSettings.GetPropertyValue<string>("osqrDefaultTableName");
            }
        }
    }
}