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
                + System.Environment.NewLine +
            " < !--- < !DOCTYPE HTML PUBLIC \" -//W3C//DTD HTML 4.0 Transitional//EN\" > ---> "
             + System.Environment.NewLine 
                + " <head> "
                + "      < !-- #BeginEditable \"Doctitle\" --> "
             + System.Environment.NewLine 

                + " <title > Peer Review Schedules</title >  "
             + System.Environment.NewLine 
                + " <meta http - equiv = \"Content-Type\" content = \"text/html; charset=iso-8859-1\" > "
            + System.Environment.NewLine
                + "< !-- #EndEditable -->"
            + System.Environment.NewLine
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
                                            + " < !-- #BeginEditable \"MainContent\" -->"
                                            + System.Environment.NewLine
                                            + "  < !---Table which holds the quick links to the program titles--->"
                                            + System.Environment.NewLine
                                            + "    <table border = \"0\" bordercolor = \"#FFFFFF\" > "
                                            + System.Environment.NewLine;
        public static string BodySection1
        {
            get { return bodySection1; }
            set { bodySection1 = value; }

        }
        #endregion
        #region Table rows 1 to 4
        private static string tablerow1 = "<tr>"+ System.Environment.NewLine
                                        + "< td valign=\"top\" align=\"CENTER\" colspan=\"4\">"+ System.Environment.NewLine
                                        + "   <a href = \"/research/docs.htm?docid=1607\" >"+ System.Environment.NewLine
                                        + "      < font color=\"#000000\" face=\"Arial, Helvetica, sans-serif\" size=\"3\">"+ System.Environment.NewLine
                                        + "        Directions for Setting Termination Dates"
                                        + "   </font>"+ System.Environment.NewLine
                                        + " </a>"+ System.Environment.NewLine
                                        + " </td>"+ System.Environment.NewLine
                                        + "</tr>"+ System.Environment.NewLine;
        public static string Tablerow1
        {
            get { return tablerow1; }
            set { tablerow1 = value; }

        }
        private static string tablerow2 = "<tr>"+ System.Environment.NewLine
                                        + "< td valign=\"top\" align=\"CENTER\" colspan=\"4\">"+ System.Environment.NewLine
                                        + "    <div align = \"left\" >"+ System.Environment.NewLine
                                        + "       < font size=\"2\" color=\"#000000\" face=\"Arial\">"+ System.Environment.NewLine
                                        + "           The dates listed are projected.Actual dates are announced at the beginning"
                                         + "           of each peer review session.For efficiency, we simply calculate"
                                           + "         the termination date as 6 months beyond the projected implementation"
                                           + "         date. If you have used a different date, there&#146;s no need to"
                                            + "         change it if it&#146;s beyond the projected implementation date."
                                            + "         This method eases our coordination in the event peer review sessions"
                                              + "       are added or postponed."
                                             + "    </font>"+ System.Environment.NewLine
                                           + "  </div>"+ System.Environment.NewLine
                                         + "</td>"+ System.Environment.NewLine
                                     + "</tr>"+ System.Environment.NewLine;

        public static string Tablerow2
        {
            get { return tablerow2; }
            set { tablerow2 = value; }

        }
        private static string tablerow3 = "<tr>"+ System.Environment.NewLine
                                        + "<td colspan=\"4\">&nbsp;<br></td>"+ System.Environment.NewLine
                                        + "</tr>"+ System.Environment.NewLine;

        public static string Tablerow3
        {
            get { return tablerow3; }
            set { tablerow3 = value; }

        }
        private static string tablerow4 = "<tr bordercolor=\"#FFFFFF\">"+ System.Environment.NewLine
                                        + "   <td width = \"25%\" >"+ System.Environment.NewLine
                                         + "       < p >"+ System.Environment.NewLine
                                      + "              < b >"+ System.Environment.NewLine
                                           + "             < font face=\"Arial, Helvetica, sans-serif\" size=\"3\">"+ System.Environment.NewLine
                                        + "                    Animal Production &amp; Protection"
                                           + "             </font>"+ System.Environment.NewLine
                                          + "          </b>"+ System.Environment.NewLine
                                         + "       </p>"+ System.Environment.NewLine
                                         + "   </td>"+ System.Environment.NewLine

                                          + "  <td width = \"25%\" >"+ System.Environment.NewLine
                                            + "    <b>"+ System.Environment.NewLine
                                           + "         < font face=\"Arial, Helvetica, sans-serif\" size=\"3\">"+ System.Environment.NewLine
                                              + "          Nutrition, Food Safety/Quality"
                                            + "        </font>"+ System.Environment.NewLine
                                             + "   </b></p>"+ System.Environment.NewLine
                                          + "  </td>"+ System.Environment.NewLine

                                          + "  <td width = \"25%\" bordercolor=\"#FFFFFF\">"+ System.Environment.NewLine
                                           + "     <p>"+ System.Environment.NewLine
                                             + "       <b>"+ System.Environment.NewLine
                                             + "           <font face = \"Arial, Helvetica, sans-serif\" size=\"3\">"+ System.Environment.NewLine
                                             + "               Natural Resources and Sustainable Agricultural Systems"
                                              + "          </font>"+ System.Environment.NewLine
                                              + "      </b>"+ System.Environment.NewLine
                                              + "  </p>"+ System.Environment.NewLine
                                          + "  </td>"+ System.Environment.NewLine

                                           + " <td width = \"25%\" >"+ System.Environment.NewLine
                                          + "      <b>"+ System.Environment.NewLine
                                             + "       <font face= \"Arial, Helvetica, sans-serif\" size= \"3\" >"+ System.Environment.NewLine
                                             + "           Crop Production &amp; Protection"
                                             + "       </font>"+ System.Environment.NewLine
                                             + "   </b></p>"+ System.Environment.NewLine
                                          + "  </td>"+ System.Environment.NewLine
                                       + " </tr>"+ System.Environment.NewLine;


        public static string Tablerow4
        {
            get { return tablerow4; }
            set { tablerow4 = value; }

        }
        #endregion
        #region Table row 5
        private static string tablerow5BeginSection = "<tr bordercolor=\"#FFFFFF\">"+ System.Environment.NewLine+
		                                              "< !---Begin local links to review information --->"+ System.Environment.NewLine;
        

        public static string Tablerow5BeginSection
        {
            get { return tablerow5BeginSection; }
            set { tablerow5BeginSection = value; }

        }
        private static string tablerow5EndSection = "</tr>"+ System.Environment.NewLine +
                                                    "</table >" +"</body>"+ System.Environment.NewLine
           + "</html>" + System.Environment.NewLine;
        public static string Tablerow5EndSection
        {
            get { return tablerow5EndSection; }
            set { tablerow5EndSection = value; }

        }

        public static DataTable SetTableRow5Td(int tdNumber)
        {
            //1.Set Access connection(using  connection string from App.config).
            string strAccessConn = ConfigurationManager.AppSettings["AccessConnection"];
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
                query = "SELECT	ID,[National Program Title]  AS NationalProgramTitle"
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('101','103','104','105','106')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 2)
            {


                query = "SELECT	ID,[National Program Title]  AS NationalProgramTitle"
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('107','108','306')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 3)
            {
                query = "SELECT	ID,[National Program Title]  AS NationalProgramTitle"
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('201', '202', '203', '204', '205', '206', '207', '211', '212', '213', '214', '215', '216', '307')"
                                     + "ORDER BY[National Program Title]";
            }
            if (tdNumber == 4)
            {
                query = "SELECT	ID,[National Program Title]  AS NationalProgramTitle"
                                     + "                    FROM    KeyDates"
                                     + " WHERE   mid([National Program Title], 4, 3) in ('301', '302','303','304','305','308')"
                                     + "ORDER BY[National Program Title]";
            }
            cmd.CommandText = query;



            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            adapter.Fill(getID);


            return getID;
        }
        
        public static string CreateHtmlStringRow5Tds(DataTable tablerow5Table, int tdNumber)
        {
            StringBuilder htmlTableRow5Tds = new StringBuilder();

           

            if (tdNumber==1)
            {
                //Building the Header row.
                htmlTableRow5Tds.Append("<tr bordercolor=\"#FFFFFF\">");
               
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                   

                }
                htmlTableRow5Tds.Append(System.Environment.NewLine);
                


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableRow5Tds.Append("<tr>");
                    foreach (DataColumn column in tablerow5Table.Columns)
                    {
                        
                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                            htmlTableRow5Tds.Append(System.Environment.NewLine);
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                        //htmlTableRow5Tds.Append(System.Environment.NewLine);
                    }
                    htmlTableRow5Tds.Append("</tr>");
                }
            }
            if (tdNumber == 2)
            {
                //Building the Header row.
                htmlTableRow5Tds.Append("<tr bordercolor=\"#FFFFFF\">");
               
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                   

                }
                htmlTableRow5Tds.Append(System.Environment.NewLine);
               


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableRow5Tds.Append("<tr>");
                    foreach (DataColumn column in tablerow5Table.Columns)
                    {

                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append(System.Environment.NewLine);
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                        //htmlTableRow5Tds.Append(System.Environment.NewLine);
                    }
                    htmlTableRow5Tds.Append("</tr>");
                }
            }
            if (tdNumber == 3)
            {
                //Building the Header row.
                htmlTableRow5Tds.Append("<tr bordercolor=\"#FFFFFF\">");

                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                  

                }
                htmlTableRow5Tds.Append(System.Environment.NewLine);
               

                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableRow5Tds.Append("<tr>");
                    foreach (DataColumn column in tablerow5Table.Columns)
                    {

                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append(System.Environment.NewLine);
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                        //htmlTableRow5Tds.Append(System.Environment.NewLine);
                    }
                    htmlTableRow5Tds.Append("</tr>");
                }
            }
            if (tdNumber == 4)
            {
                //Building the Header row.
                htmlTableRow5Tds.Append("<tr bordercolor=\"#FFFFFF\">");

                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                   

                }
                htmlTableRow5Tds.Append(System.Environment.NewLine);
               

                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableRow5Tds.Append("<tr>");
                    foreach (DataColumn column in tablerow5Table.Columns)
                    {

                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append(System.Environment.NewLine);
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                        //htmlTableRow5Tds.Append(System.Environment.NewLine);
                    }
                    htmlTableRow5Tds.Append("</tr>");
                }
            }
            return htmlTableRow5Tds.ToString();

        }
        public static DataTable SetMainPortion(int tdNumber)
        {
            //1.Set Access connection(using  connection string from App.config).
            string strAccessConn = ConfigurationManager.AppSettings["AccessConnection"];
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
                    // + "[Ad Hoc Cut - Off Date]                           AS AdHocCutOffDate"

                    + "                  FROM    KeyDates "                                    
                                     + "ORDER BY[National Program Title]";
                //
             
               


                    
            }
           
            cmd.CommandText = query;



            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            adapter.Fill(getID);


            return getID;
        }
        public static string CreateHtmlStringMainPortion(DataTable tablerow5Table, int tdNumber)
        {
            StringBuilder htmlTableMainPortion = new StringBuilder();



            if (tdNumber == 1)
            {
                //Building the Header row.
                htmlTableMainPortion.Append("<tr bordercolor=\"#FFFFFF\">");

                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableMainPortion.Append("<th>");
                    htmlTableMainPortion.Append(column.ColumnName);
                    htmlTableMainPortion.Append("</th>");


                }
                htmlTableMainPortion.Append(System.Environment.NewLine);



                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {
                    htmlTableMainPortion.Append("<tr>");
                    foreach (DataColumn column in tablerow5Table.Columns)
                    {

                        htmlTableMainPortion.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableMainPortion.Append(System.Environment.NewLine);
                        htmlTableMainPortion.Append(row[column.ColumnName]);

                        htmlTableMainPortion.Append("</td>");
                        //htmlTableRow5Tds.Append(System.Environment.NewLine);
                    }
                    htmlTableMainPortion.Append("</tr>");
                }
            }
           
            return htmlTableMainPortion.ToString();

        }
        #endregion

    }
}




