using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using USDA_ARS.Core;
namespace USDA_ARS.CoreClient
{
    public partial class CoreTest : Form
    {
        public CoreTest()
        {
            InitializeComponent();
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPullDataFromAccess_Click(object sender, EventArgs e)
        {
            PullDataFromAccess pulldata = new PullDataFromAccess();
            string finalHtmlString= pulldata.SetValues();
            //Create DataTable
            txtResult.Text= finalHtmlString;



        }

       
    }
}
