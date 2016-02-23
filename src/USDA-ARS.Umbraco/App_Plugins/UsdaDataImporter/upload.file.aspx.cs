using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.App_Plugins.UsdaDataImporter
{
    public partial class upload_file : System.Web.UI.Page
    {
        public string Id = "";
        public string defaultTableName = "Schedule Table";
        public string disableInput = "disabled=\"disabled\"";

        protected void Page_Load(object sender, EventArgs e)
        {
            var userService = ApplicationContext.Current.Services.UserService;

            var httpCtxWrapper = new System.Web.HttpContextWrapper(System.Web.HttpContext.Current);
            var umbTicket = httpCtxWrapper.GetUmbracoAuthTicket();

            if (umbTicket == null || true == string.IsNullOrEmpty(umbTicket.Name) || umbTicket.Expired)
            {

            }
            else
            {
                var user = userService.GetByUsername(umbTicket.Name);

                if (user != null)
                {
                    if (user.UserType.Name == "Administrators")
                    {
                        disableInput = "";
                    }
                }
            }


            Id = Request.QueryString.Get("id");
            IPublishedContent siteSettings = USDA_ARS.Umbraco.Extensions.Helpers.Nodes.SiteSettings();

            if (siteSettings != null)
            {
                defaultTableName = siteSettings.GetPropertyValue<string>("osqrDefaultTableName");
            }
        }
    }
}