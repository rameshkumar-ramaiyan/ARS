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
        private static string modificationHistory = "< !---"
                                        + " Author:"
                                        + " Date:"
                                        + "Purpose:"
                                        + " Modification History:"
                                        + "  Change #	Developer		Date		Remarks												Requestor"
                                        + "  ---------- - ----------------------------------------------------------------------------------------------- " +
                                     "   chg - 01      Daniel Lee      7 / 1 / 2008    Added NP 211, 215 and 216 in the query              Chris Woods" +

                                     "   chg - 02      Daniel Lee      4 / 25 / 2010   Deleted ApprovedPPOsDuetoAreaAndOSQRFromNPS field Chris Woods" +

                                       " chg - 03      Daniel Lee      3 / 20 / 2012   Added NP codes 212, 213, and 214                    Chris Woods" +

                                       " --->" +
                                       " < !--- < !DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" > --->";


        public static string ModificationHistory
        {
            get { return modificationHistory; }
            set { modificationHistory = value; }

        }
        #endregion
        #region htmlHeadSection
        private static string htmlHeadSection = "<html>"
                + " < !--- < !DOCTYPE HTML PUBLIC \" -//W3C//DTD HTML 4.0 Transitional//EN\" > ---> "
                + " <head> "
                + "      < !-- #BeginEditable \"Doctitle\" --> "

                + " < title > Peer Review Schedules</ title >  "
                + " meta http - equiv = \"Content-Type\" content = \"text/html; charset=iso-8859-1\" > "
                + "< !-- #EndEditable -->"
                + "    </ head > ";
        public static string HtmlHeadSection
        {
            get { return htmlHeadSection; }
            set { htmlHeadSection = value; }

        }
        #endregion
        #region bodySection1
        private static string bodySection1 = "<body bgcolor=\"#FFFFFF\" text=\"#000000\">"
                                            + " < !-- #BeginEditable \"MainContent\" -->"
                                            + "  < !---Table which holds the quick links to the program titles--->"
                                            + "    < table border = \"0\" bordercolor = \"#FFFFFF\" > ";
        public static string BodySection1
        {
            get { return bodySection1; }
            set { bodySection1 = value; }

        }
        #endregion
        #region Table rows 1 to 4
        private static string tablerow1 = "< tr >"
                                        + "< td valign=\"top\" align=\"CENTER\" colspan=\"4\">"
                                        + "   <a href = \"/research/docs.htm?docid=1607\" >"
                                        + "      < font color=\"#000000\" face=\"Arial, Helvetica, sans-serif\" size=\"3\">"
                                        + "        Directions for Setting Termination Dates"
                                        + "   </font>"
                                        + " </a>"
                                        + " </td>"
                                        + "</tr>";
        public static string Tablerow1
        {
            get { return tablerow1; }
            set { tablerow1 = value; }

        }
        private static string tablerow2 = "< tr >"
                                        + "< td valign=\"top\" align=\"CENTER\" colspan=\"4\">"
                                        + "    <div align = \"left\" >"
                                        + "       < font size=\"2\" color=\"#000000\" face=\"Arial\">"
                                        + "           The dates listed are projected.Actual dates are announced at the beginning"
                                         + "           of each peer review session.For efficiency, we simply calculate"
                                           + "         the termination date as 6 months beyond the projected implementation"
                                           + "         date. If you have used a different date, there&#146;s no need to"
                                            + "         change it if it&#146;s beyond the projected implementation date."
                                            + "         This method eases our coordination in the event peer review sessions"
                                              + "       are added or postponed."
                                             + "    </font>"
                                           + "  </div>"
                                         + "</td>"
                                     + "</tr>";

        public static string Tablerow2
        {
            get { return tablerow2; }
            set { tablerow2 = value; }

        }
        private static string tablerow3 = "< tr >"
                                        + "< td colspan=\"4\">&nbsp;<br></td>"
                                        + "</tr>";

        public static string Tablerow3
        {
            get { return tablerow3; }
            set { tablerow3 = value; }

        }
        private static string tablerow4 = "< tr bordercolor=\"#FFFFFF\">"
                                        + "   <td width = \"25%\" >"
                                         + "       < p >"
                                      + "              < b >"
                                           + "             < font face=\"Arial, Helvetica, sans-serif\" size=\"3\">"
                                        + "                    Animal Production &amp; Protection"
                                           + "             </font>"
                                          + "          </b>"
                                         + "       </p>"
                                         + "   </td>"

                                          + "  < td width = \"25%\" >"
                                            + "    < b >"
                                           + "         < font face=\"Arial, Helvetica, sans-serif\" size=\"3\">"
                                              + "          Nutrition, Food Safety/Quality"
                                            + "        </font>"
                                             + "   </b></p>"
                                          + "  </td>"

                                          + "  < td width = \"25%\" bordercolor=\"#FFFFFF\">"
                                           + "     < p>"
                                             + "       < b>"
                                             + "           < font face = \"Arial, Helvetica, sans-serif\" size=\"3\">"
                                             + "               Natural Resources and Sustainable Agricultural Systems"
                                              + "          </font>"
                                              + "      </b>"
                                              + "  </p>"
                                          + "  </td>"

                                           + " < td width = \"25%\" >"
                                          + "      < b >"
                                             + "       < font face= \"Arial, Helvetica, sans-serif\" size= \"3\" >"
                                             + "           Crop Production &amp; Protection"
                                             + "       </font>"
                                             + "   </b></p>"
                                          + "  </td>"
                                       + " </tr>";


        public static string Tablerow4
        {
            get { return tablerow4; }
            set { tablerow4 = value; }

        }
        #endregion
        #region Table row 5
        private static string tablerow5BeginSection = "<tr bordercolor=\"#FFFFFF\"> "+
		                                              "< !---Begin local links to review information --->";
        

        public static string Tablerow5BeginSection
        {
            get { return tablerow5BeginSection; }
            set { tablerow5BeginSection = value; }

        }
        private static string tablerow5EndSection = "</tr>" +
                                                    "</ table > ";
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


            if(tdNumber==1)
            {
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                }


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {

                    foreach (DataColumn column in tablerow5Table.Columns)
                    {
                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append("< !---Get Record information -Column 1--->");
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                    }

                }
            }
            if (tdNumber ==2)
            {
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                }


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {

                    foreach (DataColumn column in tablerow5Table.Columns)
                    {
                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append("< !---Get Record information -Column 1--->");
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                    }

                }
            }
            if (tdNumber == 3)
            {
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                }


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {

                    foreach (DataColumn column in tablerow5Table.Columns)
                    {
                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append("< !---Get Record information -Column 1--->");
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                    }

                }
            }
            if (tdNumber == 4)
            {
                foreach (DataColumn column in tablerow5Table.Columns)
                {
                    htmlTableRow5Tds.Append("<th>");
                    htmlTableRow5Tds.Append(column.ColumnName);
                    htmlTableRow5Tds.Append("</th>");
                }


                //Building the Data rows.
                foreach (DataRow row in tablerow5Table.Rows)
                {

                    foreach (DataColumn column in tablerow5Table.Columns)
                    {
                        htmlTableRow5Tds.Append("<td width=\"25 % \" valign=\"top\">");
                        htmlTableRow5Tds.Append("< !---Get Record information -Column 1--->");
                        htmlTableRow5Tds.Append(row[column.ColumnName]);

                        htmlTableRow5Tds.Append("</td>");
                    }

                }
            }

            return htmlTableRow5Tds.ToString();

        }

        #endregion

    }
}




