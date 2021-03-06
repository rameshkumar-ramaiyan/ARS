﻿using System;
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

            //row 5 begin section
            string tableRow5BeginSection = DataValues.Tablerow5BeginSection;
            //string modificationHistory = WebUtility.HtmlDecode(modificationHistoryHtml);

            //tds with queries
            DataTable row5Td1table = DataValues.SetTableRow5Td(1);
            string tablerow5Td1Html = DataValues.CreateHtmlStringRow5Tds(row5Td1table, 1);
            DataTable row5Td2table = DataValues.SetTableRow5Td(2);
            string tablerow5Td2Html = DataValues.CreateHtmlStringRow5Tds(row5Td2table, 2);
            DataTable row5Td3table = DataValues.SetTableRow5Td(3);
            string tablerow5Td3Html = DataValues.CreateHtmlStringRow5Tds(row5Td3table, 3);
            DataTable row5Td4table = DataValues.SetTableRow5Td(4);
            string tablerow5Td4Html = DataValues.CreateHtmlStringRow5Tds(row5Td4table, 4);

            string tablerow5TdsHtml = tablerow5Td1Html + tablerow5Td2Html + tablerow5Td3Html + tablerow5Td4Html;

            //row5 end section
            string tableRow5EndSection = DataValues.Tablerow5EndSection;
            string finalHtmlString = modificationHistoryHtml
                                   + System.Environment.NewLine
                                   + headSectionHtml
                                   + System.Environment.NewLine
                                   + bodySection1Html
                                   + tableRow5BeginSection
                                   + System.Environment.NewLine
                                   + tablerow5TdsHtml
                                   + System.Environment.NewLine
                                   + tableRow5EndSection
                                   + System.Environment.NewLine
                                   ;
            //store this string in db


            return finalHtmlString;


        }


    }
}




