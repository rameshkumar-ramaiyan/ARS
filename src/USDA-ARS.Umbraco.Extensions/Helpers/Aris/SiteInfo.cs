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

            string sql = "";

            sql = @"select	modecodeDash as modecode, web_label
		            from	v_ref_modecode
		
		            where 1=1";

            if (type == "title")
            {
                sql += " and web_label LIKE '%' + @query + '%'";
            }
            else if (type == "mission-statement")
            {
                sql += " and mission_statement LIKE '%' + @query + '%'";
            }
            else if (type == "state-code")
            {
                sql += " and STATE_CODE = @query";
            }

            sql += @"
		        
		        -- gets rid of non-research modecodes ('20','30','50','60','80')
		        and modecode_1 in (
							        select distinct modecode_1
							        from ref_modecode
							        where LEN(MODECODE_1) = 2
							        and status_code = 'a'
							        and modecode_1 <> '82' -- National Agricultural Library
						        )
	 	        -- gets rid of cities and support staff
	 	        and modecode_3 NOT in ('00', '02')
	 	
		        -- gets rid of office of the director
	 	        and modecode_4 <> '01'
	 	
	 	        -- gets rid of the Administrative Office in Beltsville
		        and modecodeconc NOT in ('80420520', '80420502', '80420503', '80420504')  	
	
		        order by modecode";

            locationList = db.Query<RefModeCode>(sql, new { query = query }).ToList();

            return locationList;
        }


        public static List<RefModeCode> SearchLocationByAlpha(string letter)
        {
            List<RefModeCode> locationList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = "";

            sql = @"select	modecodeDash as modecode, web_label
		                    from	v_ref_modecode
			
		                    where  left(web_label, 1) = @letter 
		
		                    
		                    -- gets rid of non-research modecodes ('20','30','50','60','80')
		                    and modecode_1 in (
							                    select distinct modecode_1
							                    from ref_modecode
							                    where LEN(MODECODE_1) = 2
							                    and status_code = 'a'
							                    and modecode_1 <> '82' -- National Agricultural Library
						                    )
	 	                    -- gets rid of cities and support staff
	 	                    and modecode_3 NOT in ('00', '02')
	 	
		                    -- gets rid of office of the director
	 	                    and modecode_4 <> '01'
	
		                     -- gets rid of the Administrative Office in Beltsville
		                    and modecodeconc NOT in ('80420520', '80420502', '80420503', '80420504')  
	
		                    order by web_label";


            locationList = db.Query<RefModeCode>(sql, new { letter = letter }).ToList();

            if (locationList != null && locationList.Any())
            {
                locationList = locationList.OrderBy(p => p.WebLabel).ToList();
            }

            return locationList;
        }
    }
}
