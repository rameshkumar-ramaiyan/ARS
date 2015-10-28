using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Xml.Serialization;
using System.Configuration;
using System.Xml;
using System.Net;

using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;
namespace USDA_ARS.Core
{
    public class PullDataFromAccess
    {



        // Setting environment values
        public string SetValues()
        {


            //1.Set Access connection (using  connection string from App.config).
            string strAccessConn = ConfigurationManager.AppSettings["AccessConnection"];

            string modificationHistoryHtml = DataValues.ModificationHistory;
            string headSectionHtml = DataValues.HtmlHeadSection;
            string bodySection1Html = DataValues.BodySection1;
            string tablerow1 = DataValues.Tablerow1;
            string tablerow2 = DataValues.Tablerow2;
            string tablerow3 = DataValues.Tablerow3;
            string tablerow4 = DataValues.Tablerow4;

            //string modificationHistory = WebUtility.HtmlDecode(modificationHistoryHtml);


            DataTable row5Td1table = DataValues.SetTableRow5Td(1);
            DataTable row5Td2table = DataValues.SetTableRow5Td(2);
            DataTable row5Td3table = DataValues.SetTableRow5Td(3);
            DataTable row5Td4table = DataValues.SetTableRow5Td(4);
            string tableRow5Section1 = DataValues.CreateHtmlStringRow5Td1(row5Td1table);
            string tablerow5 = DataValues.CreateHtmlStringRow5Td1(row5Td1table);


            string finalHtmlString = modificationHistoryHtml + headSectionHtml + bodySection1Html + tablerow5;
            return finalHtmlString;
        }


    }
}




