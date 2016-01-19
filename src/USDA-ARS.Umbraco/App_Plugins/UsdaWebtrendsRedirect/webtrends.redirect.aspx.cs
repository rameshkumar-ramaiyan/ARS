using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace USDA_ARS.Umbraco.App_Plugins.UsdaWebtrendsRedirect
{
    public partial class webtrends_redirect : System.Web.UI.Page
    {
        public string PROFILE_ID = "jOMFOeKwIC7.wlp";
        private static readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["nodeid"] != null)
            {
                int nodeId = Convert.ToInt32(Request.QueryString.Get("nodeid"));

                IContent content = _contentService.GetById(nodeId);

                if (content != null)
                {
                    if (content.Properties["webtrendsProfileID"].Value != null)
                    {
                        PROFILE_ID = content.Properties["webtrendsProfileID"].Value.ToString();

                        if (content.Properties["modeCode"].Value != null)
                        {
                            PROFILE_ID += "_" + content.Properties["modeCode"].Value.ToString() + ".wlp";
                        }
                    }
                }
            }
        }
    }
}