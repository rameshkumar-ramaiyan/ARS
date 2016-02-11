using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class SiteInfo
    {
        public string MissionStatement { get; set; }
        public string AdminDetails { get; set; }


        public SiteInfo(string modeCode)
        {
            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;
            Sql sqlPerson = null;

            if (modeCodeArray != null && modeCodeArray.Count == 4)
            {
                string where = "MODECODE_1 = '" + modeCodeArray[0] + "' AND ";
                where += "MODECODE_2 = '" + modeCodeArray[1] + "' AND ";
                where += "MODECODE_3 = '" + modeCodeArray[2] + "' AND ";
                where += "MODECODE_4 = '" + modeCodeArray[3] + "'";

                sql = new Sql()
                 .Select("*")
                 .From("REF_MODECODE")
                 .Where(where);

                RefModeCode refModeCode = db.Query<RefModeCode>(sql).FirstOrDefault();


                if (refModeCode != null)
                {
                    sqlPerson = new Sql()
                        .Select("*")
                        .From("w_people_info")
                        .Where("EMP_ID = '" + refModeCode.RlEmpId + "'");

                    PeopleInfo peopleInfo = db.Query<PeopleInfo>(sqlPerson).FirstOrDefault();

                    this.MissionStatement = refModeCode.MissionStatement;

                    if (peopleInfo != null)
                    {
                        this.AdminDetails = "<a href=\"/people-locations/person/?person-id=" + peopleInfo.PersonId + "\">";
                        this.AdminDetails += peopleInfo.LastName + ", " + peopleInfo.FirstName + " " + peopleInfo.MiddleName + "</a><br />\r\n";
                        this.AdminDetails += peopleInfo.TitleWorking + "<br />\r\n";
                        this.AdminDetails += "<a href=\"/contactus/feedback.htm?person-id=" + peopleInfo.PersonId + "\">";
                        this.AdminDetails += peopleInfo.Email + "</a><br />\r\n";
                        this.AdminDetails += "(" + peopleInfo.PhoneAreaCode + ") " + peopleInfo.Phone.Substring(0, 3) + "-" + peopleInfo.Phone.Substring(3, 4) + "<br />\r\n";
                        this.AdminDetails += refModeCode.RsAddress1 + "<br />\r\n";
                        this.AdminDetails += refModeCode.RsAddress2 + "<br />\r\n";
                        this.AdminDetails += Utilities.Strings.UppercaseFirst(peopleInfo.City) + " " + refModeCode.RsStateCode + " " + refModeCode.RsPostalCode + "<br />\r\n";
                    }
                }
            }



        }


        public static List<string> GetAlphaList()
        {
            List<string> alphaList = null;

            var db = new Database("arisPublicWebDbDSN");

            List<RefModeCode> locationList = SearchLocation("");

            if (locationList != null && locationList.Any())
            {
                alphaList = locationList.Select(p => p.WebLabel.Substring(0, 1)).Distinct().OrderBy(p => p).ToList();
            }

            return alphaList;
        }


        public static List<RefModeCode> SearchLocation(string query, string type = "title")
        {
            List<RefModeCode> locationList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            //select modecode_1 +'-' + modecode_2 + '-' + modecode_3 + '-' + modecode_4 as modecode , web_label
            //from v_locations
            //where web_label LIKE  '%test%'
            //order by modecode
            string select = @"REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_1))) + CONVERT(VARCHAR(2), RM.MODECODE_1) + REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), 
                         RM.MODECODE_2))) + CONVERT(VARCHAR(2), RM.MODECODE_2) + REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_3))) + CONVERT(VARCHAR(2), RM.MODECODE_3) + REPLICATE('0', 
                         2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_4))) + CONVERT(VARCHAR(2), RM.MODECODE_4) AS modecodeconc, REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_1))) 
                         + CONVERT(VARCHAR(2), RM.MODECODE_1) AS modecode_1, REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_2))) + CONVERT(VARCHAR(2), RM.MODECODE_2) AS modecode_2, 
                         REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), RM.MODECODE_3))) + CONVERT(VARCHAR(2), RM.MODECODE_3) AS modecode_3, REPLICATE('0', 2 - DATALENGTH(CONVERT(VARCHAR(2), 
                         RM.MODECODE_4))) + CONVERT(VARCHAR(2), RM.MODECODE_4) AS modecode_4, CASE WHEN modecode_1 IN (0, 1, 3, 5, 82) 
                         THEN CASE WHEN modecode_2 = 0 THEN modecode_1_desc WHEN modecode_3 = 0 THEN (CASE WHEN (modecode_2_desc LIKE '%OFFICE%' OR
                         modecode_2_desc LIKE '%support%' OR
                         modecode_3_desc LIKE '%services%') AND modecode_1 <> 1 AND modecode_2 < 5 THEN MODECODE_1_DESC + ': ' + MODECODE_2_DESC ELSE MODECODE_2_DESC END) 
                         WHEN modecode_4 = 0 THEN (CASE WHEN (modecode_3_desc LIKE '%OFFICE of the%' OR
                         modecode_3 = 1 OR
                         modecode_3_desc LIKE '%support%' OR
                         modecode_3_desc LIKE '%services%') THEN MODECODE_2_DESC + ': ' + MODECODE_3_DESC ELSE MODECODE_3_DESC END) ELSE (CASE WHEN (modecode_4_desc LIKE '%OFFICE%' OR
                         modecode_4_desc LIKE '%support%') THEN MODECODE_3_DESC + ': ' + MODECODE_4_DESC ELSE MODECODE_4_DESC END) 
                         END ELSE CASE WHEN modecode_2 = 0 THEN modecode_1_desc WHEN modecode_3 = 0 THEN (CASE WHEN (modecode_2_desc LIKE '%OFFICE%' OR
                         modecode_2_desc LIKE '%support%' OR
                         modecode_3_desc LIKE '%services%') AND modecode_1 <> 1 AND modecode_2 < 5 THEN MODECODE_1_DESC + ': ' + MODECODE_2_DESC ELSE MODECODE_2_DESC END) 
                         WHEN modecode_4 = 0 THEN (CASE WHEN (modecode_3_desc LIKE '%OFFICE%' OR
                         modecode_3_desc LIKE '%support%' OR
                         modecode_3_desc LIKE '%services%') THEN MODECODE_2_DESC + ': ' + MODECODE_3_DESC ELSE MODECODE_3_DESC END) 
                         ELSE MODECODE_3_DESC + ': ' + MODECODE_4_DESC END END AS Web_Label, RM.MODECODE_AGENCY, RM.MODECODE_REGION, RM.MODECODE_AREA, RM.MODECODE_LEVEL_7, 
                         RM.MODECODE_LEVEL_8, RM.STATUS_CODE, RM.STATUS_DATE, RM.DEPT_CODE1, RM.DEPT_CODE2, RM.INST_CODE1, RM.INST_CODE2, RM.RESEARCH_UNIT, RM.MODECODE_1_DESC, 
                         RM.MODECODE_2_DESC, RM.MODECODE_3_DESC, RM.MODECODE_4_DESC, RM.MODECODE_L7_DESC, RM.MODECODE_L8_DESC, RM.FACILITY_NAME, RM.DATE_CREATED, RM.USER_CREATED, 
                         RM.DATE_LAST_MOD, RM.USER_LAST_MOD, RM.RL_EMP_ID, RM.RL_EMAIL, RM.RL_FAX, RM.RL_TITLE, RM.RL_PHONE, RM.ADD_LINE_1, RM.ADD_LINE_2, dbo.fn_title_case(RM.CITY) AS city, 
                         RM.STATE_CODE, RM.POSTAL_CODE, RM.COUNTRY_CODE, RM.MISSION_STATEMENT, RM.COLOCATION_ARS_FACILTIY, RM.COLOCATION_USDA_AGENCY, RM.COLOCATION_UNIVERSITY, 
                         RM.COLOCATION_PRIVATE, RM.OTHER_AGENCY_COLOCATED_ARS, RM.RESEARCH_AT_MU, RM.RESEARCH_AT_LOC, RM.RESEARCH_AT_AREA, RM.RESEARCH_AT_ANOTHER_AREA, 
                         RM.RESEARCH_NON_NEEDED";
            string where = "(RM.STATUS_CODE = 'a') AND (NOT (RM.MODECODE_1 = 3)) OR (RM.STATUS_CODE = 'a') AND (NOT (RM.MODECODE_2 = 22))";


            sql = new Sql()
             .Select(select)
             .From("dbo.REF_MODECODE AS RM")
             .Where(where)
             .OrderBy("modecodeconc");

            locationList = db.Query<RefModeCode>(sql).ToList();

            if (locationList != null && locationList.Any())
            {
                if (type == "title")
                {
                    locationList = locationList.Where(p => p.WebLabel.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                else if (type == "mission-statement")
                {
                    locationList = locationList.Where(p => p.MissionStatement != null && p.MissionStatement.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                else if (type == "alpha")
                {
                    locationList = locationList.Where(p => p.WebLabel.IndexOf(query, StringComparison.OrdinalIgnoreCase) == 0).ToList();
                    locationList = locationList.OrderBy(p => p.WebLabel).ToList();
                }
            }

            return locationList;
        }
    }
}
