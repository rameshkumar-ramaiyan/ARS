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
using System.Data.SqlClient;

namespace USDA_ARS.Core
{
    public class DataValues
    {


        #region modificationHistory
        private static string modificationHistory = "< !---" + System.Environment.NewLine
                                        + " Author:" + System.Environment.NewLine
                                        + " Date:" + System.Environment.NewLine
                                        + "Purpose:" + System.Environment.NewLine
                                        + " Modification History:" + System.Environment.NewLine
                                        + "  Change #	Developer		Date		Remarks											Requestor" + System.Environment.NewLine
                                        + "  ---------- - ----------------------------------------------------------------------------------------------- " + System.Environment.NewLine +
                                     "   chg - 01      Daniel Lee      7 / 1 / 2008    Added NP 211, 215 and 216 in the query              Chris Woods" + System.Environment.NewLine +

                                     "   chg - 02      Daniel Lee      4 / 25 / 2010   Deleted ApprovedPPOsDuetoAreaAndOSQRFromNPS field Chris Woods" + System.Environment.NewLine +

                                       " chg - 03      Daniel Lee      3 / 20 / 2012   Added NP codes 212, 213, and 214                    Chris Woods" + System.Environment.NewLine +

                                       " --->" + System.Environment.NewLine +
                                       " < !--- < !DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" > --->" + System.Environment.NewLine;


        public static string ModificationHistory
        {
            get { return modificationHistory; }
            set { modificationHistory = value; }

        }
        #endregion
        #region htmlHeadSection
        private static string htmlHeadSection = "<html>"
             //    + System.Environment.NewLine +
             //" < !DOCTYPE HTML PUBLIC \" -//W3C//DTD HTML 4.0 Transitional//EN\" > "
             + System.Environment.NewLine
                + " <head> "
                //   + "      < !-- #BeginEditable \"Doctitle\" --> "
                //+ System.Environment.NewLine

                + " <title > Peer Review Schedules</title >  "
             + System.Environment.NewLine
                + " <meta http - equiv = \"Content-Type\" content = \"text/html; charset=iso-8859-1\" > "
            + System.Environment.NewLine
                //    + "< !-- #EndEditable -->"
                //+ System.Environment.NewLine


                + "    </head > "
             + System.Environment.NewLine;
        public static string HtmlHeadSection
        {
            get { return htmlHeadSection; }
            set { htmlHeadSection = value; }

        }
        #endregion
        #region bodySection1
        private static string bodySection1 = "<body bgcolor=\"#FFFFFF\" text=\"#000000\">"
                                            + System.Environment.NewLine
            +

                                                         " <style>" + System.Environment.NewLine +
                                            ".table - main { width: 100%; } " + System.Environment.NewLine +
                                            ".table-main-row { width: 100%; clear: both; } " + System.Environment.NewLine +
                                            ".table-main-header { background-color: #ddd; padding: 3px;  }" + System.Environment.NewLine +
                                            ".table-main-cell-left { width: 55%; float: left; padding-top: 3px;padding-bottom: 3px; }" + System.Environment.NewLine +
                                            ".table-main-cell-right { width: 40%; float: left;  padding-top: 3px;padding-bottom: 3px; }" + System.Environment.NewLine +
                                            ".table-main-cell-full { width: 100%; float: left;  padding-bottom: 3px; }" + System.Environment.NewLine +
                                            ".table-main-cell-center { width: 100%; float: center;  padding-top: 3px;padding-bottom: 3px; }" + System.Environment.NewLine +

                                            ".table" + System.Environment.NewLine +
                                            "{" + System.Environment.NewLine +
                                              " display: table;" + System.Environment.NewLine +
                                               "}" + System.Environment.NewLine +

                                            ".tablerow" + System.Environment.NewLine +
                                            "{" + System.Environment.NewLine +
                                               "display: table-row; " + System.Environment.NewLine +
                                            "}" + System.Environment.NewLine +

                                            ".tablecell" + System.Environment.NewLine +
                                            "{" + System.Environment.NewLine +
                                               "display: table-cell;" + System.Environment.NewLine +
                                            "}" + System.Environment.NewLine +
                                            "</style>" + System.Environment.NewLine
                                            //+ " < !-- #BeginEditable \"MainContent\" -->"
                                            //+ System.Environment.NewLine
                                            //+ "  < !---Table which holds the quick links to the program titles--->"
                                            //+ System.Environment.NewLine
                                            ;


        public static string BodySection1
        {
            get { return bodySection1; }
            set { bodySection1 = value; }

        }
        #endregion
        #region Table rows 1 to 4
        private static string tablerow1 =
                                                "<div class=\"table-main-row\">" + System.Environment.NewLine
                                        + "<div class=\"table-main-cell-center\" align=\"CENTER\" colspan=\"4\">" + System.Environment.NewLine
                                        + "   <a href = \"/research/docs.htm?docid=1607\" >" + System.Environment.NewLine
                                        + "      <font color=\"#000000\" face=\"Arial, Helvetica, sans-serif\" size=\"3\">" + System.Environment.NewLine
                                        + "        Directions for Setting Termination Dates"
                                        + "   </font>" + System.Environment.NewLine
                                        + " </a>" + System.Environment.NewLine
                                        + " </div>" + System.Environment.NewLine
                                        + "</div>" + System.Environment.NewLine;
        public static string Tablerow1
        {
            get { return tablerow1; }
            set { tablerow1 = value; }

        }
        private static string tablerow2 =
                                             "<div class=\"table-main-row\">" + System.Environment.NewLine
                                        + "<div class=\"tablecell\" valign=\"top\" align=\"CENTER\" colspan=\"4\">" + System.Environment.NewLine
                                        + "    <div align = \"left\" >" + System.Environment.NewLine
                                        + "       <font size=\"2\" color=\"#000000\" face=\"Arial\">" + System.Environment.NewLine
                                        + "           The dates listed are projected.Actual dates are announced at the beginning"
                                         + "           of each peer review session.For efficiency, we simply calculate"
                                           + "         the termination date as 6 months beyond the projected implementation"
                                           + "         date. If you have used a different date, there&#146;s no need to"
                                            + "         change it if it&#146;s beyond the projected implementation date."
                                            + "         This method eases our coordination in the event peer review sessions"
                                              + "       are added or postponed."
                                             + "    </font>" + System.Environment.NewLine
                                           + "  </div>" + System.Environment.NewLine
                                         + "</div>" + System.Environment.NewLine
                                     + "</div>" + System.Environment.NewLine;


        public static string Tablerow2
        {
            get { return tablerow2; }
            set { tablerow2 = value; }

        }
        private static string tablerow3 = "<div class=\"table-main-row\">" + System.Environment.NewLine
                                        + "<div class=\"tablecell\" colspan=\"4\">&nbsp;<br></div>" + System.Environment.NewLine
                                        + "</div>" + System.Environment.NewLine;

        public static string Tablerow3
        {
            get { return tablerow3; }
            set { tablerow3 = value; }

        }
        private static string tablerow4 = "<div class=\"table-main-row\" bordercolor=\"#FFFFFF\">" + System.Environment.NewLine
                                        + "   <div class=\"tablecell\"   width = \"15%\" >" + System.Environment.NewLine
                                         + "       <p>" + System.Environment.NewLine
                                      + "              <b>" + System.Environment.NewLine
                                           + "             <font face=\"Arial, Helvetica, sans-serif\" size=\"3\">" + System.Environment.NewLine
                                        + "                    Animal Production &amp; Protection"
                                           + "             </font>" + System.Environment.NewLine
                                          + "          </b>" + System.Environment.NewLine
                                         + "       </p>" + System.Environment.NewLine
                                         + "   </div>" + System.Environment.NewLine

                                          + "  <div class=\"tablecell\"  width = \"25%\" >" + System.Environment.NewLine
                                            + "    <b>" + System.Environment.NewLine
                                           + "         <font face=\"Arial, Helvetica, sans-serif\" size=\"3\">" + System.Environment.NewLine
                                              + "          Nutrition, Food Safety/Quality"
                                            + "        </font>" + System.Environment.NewLine
                                             + "   </b></p>" + System.Environment.NewLine
                                          + "  </div>" + System.Environment.NewLine

                                          + "  <div class=\"tablecell\" width = \"25%\" bordercolor=\"#FFFFFF\">" + System.Environment.NewLine
                                           + "     <p>" + System.Environment.NewLine
                                             + "       <b>" + System.Environment.NewLine
                                             + "           <font face = \"Arial, Helvetica, sans-serif\" size=\"3\">" + System.Environment.NewLine
                                             + "               Natural Resources and Sustainable Agricultural Systems"
                                              + "          </font>" + System.Environment.NewLine
                                              + "      </b>" + System.Environment.NewLine
                                              + "  </p>" + System.Environment.NewLine
                                          + "  </div>" + System.Environment.NewLine

                                           + " <div class=\"tablecell\" width = \"25%\" >" + System.Environment.NewLine
                                          + "      <b>" + System.Environment.NewLine
                                             + "       <font face= \"Arial, Helvetica, sans-serif\" size= \"3\" >" + System.Environment.NewLine
                                             + "           Crop Production &amp; Protection"
                                             + "       </font>" + System.Environment.NewLine
                                             + "   </b></p>" + System.Environment.NewLine
                                          + "  </div>" + System.Environment.NewLine
                                       + " </div>" + System.Environment.NewLine;



        public static string Tablerow4
        {
            get { return tablerow4; }
            set { tablerow4 = value; }

        }
        #endregion
        #region Table row 5
        private static string tablerow5 = "<div class=\"table-main-row\"  bordercolor=\"#FFFFFF\">" + System.Environment.NewLine
                                        + "   <div class=\"tablecell\"  width = \"25%\" >" + System.Environment.NewLine
                                        + ConvertDataTableToHTML(SetTableRow5Td(1), 1)
                                        + "   </div>" + System.Environment.NewLine
                                        + "  <div class=\"tablecell\"  width = \"25%\" >" + System.Environment.NewLine
                                        + ConvertDataTableToHTML(SetTableRow5Td(2), 2)
                                        + "  </div>" + System.Environment.NewLine
                                        + "   <div class=\"tablecell\"  width = \"25%\" >" + System.Environment.NewLine
                                        + ConvertDataTableToHTML(SetTableRow5Td(3), 3)
                                        + "   </div>" + System.Environment.NewLine
                                        + "  <div class=\"tablecell\"  width = \"25%\" >" + System.Environment.NewLine
                                        + ConvertDataTableToHTML(SetTableRow5Td(4), 4)
                                        + "  </div>" + System.Environment.NewLine
                                        + " </div>" + System.Environment.NewLine
                                        + " </div>" + System.Environment.NewLine;

        public static string Tablerow5
        {
            get { return tablerow5; }
            set { tablerow5 = value; }

        }




        private static string tablerow5BeginSection = "<div class=\"table-main-row\"  bordercolor=\"#FFFFFF\">" + System.Environment.NewLine
                                                      //+
                                                      //"< !---Begin local links to review information --->" + System.Environment.NewLine
                                                      ;


        public static string Tablerow5BeginSection
        {
            get { return tablerow5BeginSection; }
            set { tablerow5BeginSection = value; }

        }
        private static string tablerow5EndSection = "</body>" + System.Environment.NewLine
           + "</html>" + System.Environment.NewLine;
        public static string Tablerow5EndSection
        {
            get { return tablerow5EndSection; }
            set { tablerow5EndSection = value; }

        }

        public static DataTable SetTableRow5Td(int tdNumber)
        {
            //1.Set Access connection(using  connection string from App.config).
            // string strAccessConn = ConfigurationManager.AppSettings["AccessConnection"];
            string strAccessConn = PullDataFromAccess.AccessConnectionString;
            //private static string tablerow5 = 
            //2.select values from keydates table.
            DataTable getID = new DataTable();
            OleDbConnection conn = new OleDbConnection(strAccessConn);
            conn.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;
            string query = "";
            if (tdNumber == 1)
            {
                query = "SELECT	[National Program Title]  "
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('101','103','104','105','106')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 2)
            {


                query = "SELECT	[National Program Title]  "
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('107','108','306')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 3)
            {
                query = "SELECT	[National Program Title]  "
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('201', '202', '203', '204', '205', '206', '207', '211', '212', '213', '214', '215', '216', '307')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 4)
            {
                query = "SELECT	[National Program Title]  "
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('301', '302','303','304','305','308')"
                                     + "ORDER BY[National Program Title]";
            }
            cmd.CommandText = query;



            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            adapter.Fill(getID);


            return getID;
        }

        //public static string CreateHtmlStringRow5Tds(DataTable tablerow5Table, int tdNumber)
        //{
        //    StringBuilder htmlTableRow5Tds = new StringBuilder();

        //    //Building the Header row.
        //    htmlTableRow5Tds.Append("<div class=\"table-main-row\"  bordercolor=\"#FFFFFF\">");

        //    if (tdNumber == 1)
        //    {

        //        htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //        //Building the Data rows.
        //        foreach (DataRow row in tablerow5Table.Rows)
        //        {
        //            htmlTableRow5Tds.Append("<div class=\"table-main-row\" >");
        //            foreach (DataColumn column in tablerow5Table.Columns)
        //            {

        //                htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");

        //                htmlTableRow5Tds.Append(System.Environment.NewLine);

        //                 //htmlTableRow5Tds.Append("<a href=\"#C4\">");
        //                htmlTableRow5Tds.Append("<a href =\"#C4\">"+row[column.ColumnName]+"</a>");
        //               // htmlTableRow5Tds.Append("</a>");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);

        //                htmlTableRow5Tds.Append("</div>");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //            }
        //            htmlTableRow5Tds.Append("</div>");
        //        }
        //        htmlTableRow5Tds.Append("</div>");
        //    }
        //        if (tdNumber == 2)
        //    {


        //        htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //        //Building the Data rows.
        //        foreach (DataRow row in tablerow5Table.Rows)
        //        {
        //            htmlTableRow5Tds.Append("<div class=\"table-main-row\" >");
        //            foreach (DataColumn column in tablerow5Table.Columns)
        //            {

        //                htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //                htmlTableRow5Tds.Append(row[column.ColumnName]);
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //                htmlTableRow5Tds.Append("</div>");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //            }
        //            htmlTableRow5Tds.Append("</div>");
        //        }
        //        htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //    }
        //    if (tdNumber == 3)
        //    {

        //        //Building the Data rows.
        //        foreach (DataRow row in tablerow5Table.Rows)
        //        {
        //            htmlTableRow5Tds.Append("<div class=\"table-main-row\" >");
        //            foreach (DataColumn column in tablerow5Table.Columns)
        //            {

        //                htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //                htmlTableRow5Tds.Append(row[column.ColumnName]);

        //                htmlTableRow5Tds.Append("</div>");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //            }
        //            htmlTableRow5Tds.Append("</div>");
        //        }
        //    }
        //    if (tdNumber == 4)
        //    {
        //        //Building the Data rows.
        //        foreach (DataRow row in tablerow5Table.Rows)
        //        {
        //            htmlTableRow5Tds.Append("<div class=\"table-main-row\" >");
        //            foreach (DataColumn column in tablerow5Table.Columns)
        //            {

        //                htmlTableRow5Tds.Append("<div class=\"tablecell\"  width=\"25 % \" valign=\"top\">");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //                htmlTableRow5Tds.Append(row[column.ColumnName]);

        //                htmlTableRow5Tds.Append("</div>");
        //                htmlTableRow5Tds.Append(System.Environment.NewLine);
        //            }
        //            htmlTableRow5Tds.Append("</div>");
        //        }
        //    }
        //    //Building the Header row.
        //    htmlTableRow5Tds.Append("</div>");
        //    return htmlTableRow5Tds.ToString();

        //}
        public static DataTable SetMainPortion(int tdNumber)
        {
            //1.Set Access connection(using  connection string from App.config).
            //string strAccessConn = ConfigurationManager.AppSettings["AccessConnection"];
            string strAccessConn = PullDataFromAccess.AccessConnectionString;
            //private static string tablerow5 = 
            //2.select values from keydates table.
            DataTable getID = new DataTable();
            OleDbConnection conn = new OleDbConnection(strAccessConn);
            conn.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;
            string query = "";
            if (tdNumber == 1)
            {
                query = "SELECT	ID,"
                    + "[National Program Title]                        AS NationalProgramTitle,"
                    + "[Termination Date]                              AS TerminationDate,"
                    + "[Program Analyst]                               AS ProgramAssistant,"
                    + "[Number of Projects in the Review]              AS RoundNumberofProjectsintheReview,"
                    + "[Planned Duration]                              AS PlannedDuration,"
                    + "[Status of Reviews]                             AS statusOfReviews,"
                    + "[Concurrence Memo Due to Area Director]         AS ConcurrenceMemoDuetoAreaDirector,"
                    + "[PDRAMs Due to Area & OSQR with Schedule]       AS PDRAMsDueToAreaOSQRWithSchedule,"
                    + "[Conflicts of Interest Lists Due To OSQR]       AS ConflictsofInterestListDuetoOSQR,"

                    + "[Project Plans Due to OSQR]                     AS ProjectPlansDueToOSQR,"
                    + "[Review Period]                                 AS ReviewPeriod,"

                    + "[Project's Targeted Implementation Date]		AS ProjectsTargetedImplementationDate"
                    + ",[Ad Hoc Cut-Off Date]                           AS AdHocCutOffDate"

                    + "                  FROM    KeyDates "
                                     + "ORDER BY[National Program Title]";
                //





            }

            cmd.CommandText = query;



            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            adapter.Fill(getID);


            return getID;
        }
        public static string CreateHtmlStringMainPortion1(DataTable tablerow5Table, int tdNumber)
        {
            StringBuilder htmlTableMainPortion = new StringBuilder();



            if (tdNumber == 1)
            {




                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\" >");
                    // 1.first create a table with to tds
                    //use for loop for entering values
                    DataColumn column = new DataColumn();
                    // for (int i=0;i<= tablerow5Table.Columns.Count;i++)
                    //foreach (DataColumn column in tablerow5Table.Columns)
                    //  {
                    //Building the Data rows.
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    // htmlTableMainPortion.Append(" <div class=\"table\" cellpadding=\"2\" width=\"100%\" border=\"0\" >");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\" >  ");

                    htmlTableMainPortion.Append(" <div class=\"tablecell\" colspan=\"2\"   > ");
                    //htmlTableMainPortion.Append(" <div class=\"tablecell\" colspan=\"2\"  > ");
                    htmlTableMainPortion.Append(" <b>");
                    htmlTableMainPortion.Append("Review Title  ");

                    htmlTableMainPortion.Append("<d id =\"" + row[tablerow5Table.Columns[1].ColumnName].ToString().Replace(" ", string.Empty) + "\"" + ">");
                    // htmlTableMainPortion.Append("<a href=\"#\" id =\"" + row[tablerow5Table.Columns[1].ColumnName].ToString().Replace(" ", string.Empty) + "\"" + ">");
                    //NP 103 Animal Health Panel Review
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[1].ColumnName]);

                    // htmlTableMainPortion.Append("</a>");
                    htmlTableMainPortion.Append(" </b>");

                    // htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    //htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    ////htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");


                    htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <b>Termination Date: &nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[2].ColumnName]);

                    htmlTableMainPortion.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                    //// htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<b>Program Analyst:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[3].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Number of Projects in the Review:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[4].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    //// htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append(" &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    htmlTableMainPortion.Append("<b>Planned Duration:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[5].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Status of Reviews:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[6].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Concurrence Memo Due to Area Director:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  </b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[7].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>PDRAMs Due to Area & OSQR with Schedule:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[8].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Conflicts of Interest Lists Due To OSQR:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[9].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Project Plans Due to OSQR:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[10].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b>Review Period:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[11].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<b> Project's Targeted Implementation Date:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[12].ColumnName]);
                    htmlTableMainPortion.Append("<br>");
                    //htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<b> Ad Hoc Cut - Off Date:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append("<br>");
                    //htmlTableMainPortion.Append(row[tablerow5Table.Columns[13].ColumnName]);


                    //  }
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");
                    //// htmlTableMainPortion.Append("</div>");
                    ////   htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    //////htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");

                    ////htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<br>");
                }
            }

            return htmlTableMainPortion.ToString();

        }
        public static string CreateHtmlStringMainPortion(DataTable tablerow5Table, int tdNumber)
        {
            StringBuilder htmlTableMainPortion = new StringBuilder();



            if (tdNumber == 1)
            {



                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\" >  ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-header\" colspan=\"2\"   > ");
                    //htmlTableMainPortion.Append(" <div class=\"tablecell\" colspan=\"2\"  > ");
                    htmlTableMainPortion.Append(" <b>");
                    htmlTableMainPortion.Append("Review Title  ");

                    htmlTableMainPortion.Append("<a href=\"#\" id =\"" + row[tablerow5Table.Columns[1].ColumnName].ToString().Replace(" ", string.Empty) + "\"" + ">");
                    htmlTableMainPortion.Append("</a>");
                    //NP 103 Animal Health Panel Review
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[1].ColumnName]);


                    htmlTableMainPortion.Append(" </b>");

                    // htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");

                    htmlTableMainPortion.Append(" <b>Termination Date: &nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[2].ColumnName]);

                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");

                    //// htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<b>Program Analyst:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[3].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>Number of Projects in the Review:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[4].ColumnName]);
                    htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append("<b>Planned Duration:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[5].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-full\"> ");
                    htmlTableMainPortion.Append("<b>Status of Reviews:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[6].ColumnName]);

                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>Concurrence Memo Due to Area Director: </b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[7].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>PDRAMs Due to Area & OSQR with Schedule:</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[8].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>Conflicts of Interest Lists Due To OSQR:</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[9].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>Project Plans Due to OSQR:&nbsp;</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[10].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b>Review Period:&nbsp;</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[11].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    htmlTableMainPortion.Append("<b> Project's Targeted Implementation Date:&nbsp;</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-right\"> ");

                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[12].ColumnName]);
                    htmlTableMainPortion.Append("</div>"); htmlTableMainPortion.Append("</div>");
                    ////htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");

                    htmlTableMainPortion.Append(" <div class=\"table-main-row\"> ");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-left\"> ");
                    //htmlTableMainPortion.Append(" <div class=\"tablecell\"> ");
                    htmlTableMainPortion.Append("<b> Ad Hoc Cut - Off Date:&nbsp;&nbsp;</b>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append(" <div class=\"table-main-cell-full\"> ");
                    htmlTableMainPortion.Append(row[tablerow5Table.Columns[13].ColumnName]);
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("</div>");
                    htmlTableMainPortion.Append("</div>");

                    //htmlTableMainPortion.Append(row[tablerow5Table.Columns[13].ColumnName]);


                    //  }


                    htmlTableMainPortion.Append("<br>");
                    htmlTableMainPortion.Append("<br>");

                }
            }

            return htmlTableMainPortion.ToString();

        }
        public static void StoreHtmlStringInSQLDB(string htmlString)
        {
            //1.Set Access connection (using  connection string from App.config).

            string strSqlConn = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ToString();




            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = strSqlConn;
            SqlCommand cmd = new SqlCommand("dbo.usp_InsertMegaStatusHTMLString", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@HtmlString", htmlString);
            connection.Open();
            cmd.ExecuteNonQuery();

        }
        #endregion
        public static string ConvertDataTableToHTML(DataTable dt, int columnNumber)
        {
            string columnHeader = "";
            string html = "<div class=\"table\">";
            //add header row
            html += "<div class=\"table-main-row\">";
            for (int i = 0; i < dt.Columns.Count; i++)

                if (columnNumber == 1)
                {
                    columnHeader = "Animal Production & Protection";

                }
            if (columnNumber == 2)
            {
                columnHeader = "Nutrition, Food Safety/ Quality";
            }
            if (columnNumber == 3)
            {
                columnHeader = "Natural Resources and Sustainable Agricultural Systems";
            }
            if (columnNumber == 4)
            {
                columnHeader = "Crop Production & Protection";
            }
            html += "<div class=\"tablecell\">"

                                         + "       <p>" + System.Environment.NewLine
                                      + "              <b>" + System.Environment.NewLine
                                           + "             <font face=\"Arial, Helvetica, sans-serif\" size=\"2\">" + System.Environment.NewLine
                                       + columnHeader
                                           + "             </font>" + System.Environment.NewLine
                                          + "          </b>" + System.Environment.NewLine
                                         + "       </p>" + System.Environment.NewLine

                 + "</div>";

            html += "</div>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += " <div class=\"table-main-row\">";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += " <div class=\"tablecell\">" + "<a href=\"#" + dt.Rows[i][j].ToString().Replace(" ", string.Empty) + "\">" + dt.Rows[i][j].ToString().Replace("NP", "").Replace("Panel Review", "") + "</a>" + "</div>";
                html += "</div>";

            }
            html += "</div>";
            return html;
        }
    }
}




