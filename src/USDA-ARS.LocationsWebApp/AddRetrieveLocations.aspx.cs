using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using System.Data.SqlClient;

using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;
using USDA_ARS.LocationsWebApp.DL;

namespace USDA_ARS.LocationsWebApp
{
    public partial class AddRetrieveLocations : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnAreaRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allAreas = new System.Data.DataTable();
            allAreas = AddRetrieveLocationsDL.GetAllAreas();
            lblMessage.Text = allAreas.Rows.Count.ToString() + " Areas retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allAreas;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }

        protected void btnCityRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allCities = new System.Data.DataTable();
            int parentAreaModeCode =0;
            if (!string.IsNullOrEmpty(txtParentAreaModeCode.Text))
            {
                 parentAreaModeCode = Convert.ToInt32(txtParentAreaModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent Area Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            allCities = AddRetrieveLocationsDL.GetAllCities(parentAreaModeCode);
            lblMessage.Text = allCities.Rows.Count.ToString() + " Cities retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allCities;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }

        protected void btnResearchCenterRetrieveAll_Click(object sender, EventArgs e)
        {
            Locations locations = new Locations();
            string ConnectionString = AddRetrieveLocationsDL.LocationConnectionString;
            //Locations allAreas = new Locations();
            System.Data.DataTable allResearchCenters = new System.Data.DataTable();
            int parentAreaModeCode =0;
            int parentCityModeCode =0;
            int parentResearchCenterModeCode = 0;
            //txtParentAreaModeCode
            if (!string.IsNullOrEmpty(txtRCParentAreaModeCode.Text))
            {
                parentAreaModeCode = Convert.ToInt32(txtRCParentAreaModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent Area Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            //txtRCParentCityModeCode
            if (!string.IsNullOrEmpty(txtRCParentCityModeCode.Text))
            {
                parentCityModeCode = Convert.ToInt32(txtRCParentCityModeCode.Text);
            }
            else
            {
                lblMessage.Text = "Please enter Parent City Mode Code.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                //txtParentAreaModeCode.Focus();
                return;
            }
            //txtParenRCModeCode
            if (!string.IsNullOrEmpty(txtParenRCModeCode.Text))
            {
                parentResearchCenterModeCode = Convert.ToInt32(txtParenRCModeCode.Text);
            }
            
            allResearchCenters = AddRetrieveLocationsDL.GetAllResearchCenters(parentAreaModeCode, parentCityModeCode, parentResearchCenterModeCode);
            lblMessage.Text = allResearchCenters.Rows.Count.ToString() + " Research Centers retrieved successfully.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
            gvAreas.DataSource = allResearchCenters;
            gvAreas.DataBind();
            gvAreas.Visible = true;
        }
    }
}