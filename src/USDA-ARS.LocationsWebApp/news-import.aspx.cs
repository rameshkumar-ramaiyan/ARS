using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using USDA_ARS.Umbraco.Extensions.Models.Import;

namespace USDA_ARS.LocationsWebApp
{
    public partial class news_import : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Import_Click(object sender, EventArgs e)
        {
            DL.NewsImport.ImportNews();
        }


    }
}