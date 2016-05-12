using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ImportDocsProject = USDA_ARS.ImportDocs;
namespace USDA_ARS.ImportDocsClient
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btnImportDocsTemp_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            dt = ImportDocsProject.Program.ImportDocsTemp();


        }
    }
}