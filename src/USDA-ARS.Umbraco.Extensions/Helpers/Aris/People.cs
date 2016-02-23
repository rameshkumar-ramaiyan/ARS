using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class People
    {
        public static List<PeopleInfo> GetPeople(string modeCode)
        {
            List<PeopleInfo> peopleList = null;

            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            if (modeCodeArray != null && modeCodeArray.Count == 4)
            {
                string where = "MODECODE_1 = '" + modeCodeArray[0] + "' AND ";
                where += "MODECODE_2 = '" + modeCodeArray[1] + "' AND ";
                where += "MODECODE_3 = '" + modeCodeArray[2] + "' AND ";
                where += "MODECODE_4 = '" + modeCodeArray[3] + "'";

                sql = new Sql()
                 .Select("*")
                 .From("w_people_info")
                 .Where(where);

                peopleList = db.Query<PeopleInfo>(sql).ToList();
            }

            if (peopleList != null && peopleList.Count > 0)
            {
                peopleList = peopleList.OrderBy(p => p.LastName).ToList();
            }

            return peopleList;
        }


        public static List<PeopleByCity> GetPeopleByCity(string modeCode)
        {
            List<PeopleByCity> peopleList = null;
            
            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            if (modeCodeArray != null && modeCodeArray.Count == 4)
            {
                string modeCodeWhere = modeCodeArray[0];

                if (modeCodeArray[1] != "00")
                {
                    modeCodeWhere += modeCodeArray[1];
                }
                if (modeCodeArray[2] != "00")
                {
                    modeCodeWhere += modeCodeArray[2];
                }
                if (modeCodeArray[3] != "00")
                {
                    modeCodeWhere += modeCodeArray[3];
                }

                modeCodeWhere += "%";


                string sqlStr = @"

	                            SELECT	v.mySiteCode,
			            v.modecodeconc,
			            v.personid, 
			            v.perlname, 
			            v.perfname, 
			            v.percommonname, 
			            v.workingtitle,
			            v.EMail, 
			            v.deskareacode,
			            v.DeskPhone,
			            v.deskext,			
			            r.city,
			            r.state_code,

			            case
				            when right(v.modecodeconc, 4) = '0100' 										then r.modecode_2_desc

				            -- turn off sites
				            when v.mySiteCode <> v.modecodeconc and right(v.modecodeconc, 6) = '000000'	then r.modecode_1_desc
				            when v.mySiteCode <> v.modecodeconc and right(v.modecodeconc, 4) = '0000'	then r.modecode_2_desc
				            when v.mySiteCode <> v.modecodeconc and right(v.modecodeconc, 2) = '00'		then r.modecode_3_desc

				            when right(v.mySiteCode, 6) = '000000'										then r.modecode_1_desc
				            when right(v.mySiteCode, 4) = '0000'										then r.modecode_2_desc
				            when right(v.mySiteCode, 2) = '00'											then r.modecode_3_desc
				
				            else 									 	 									 r.modecode_4_desc
			            end 
				            as siteLabel,
					
			            ( 	
				            substring(mySiteCode, 1, 2) + '-' + 
				            substring(mySiteCode, 3, 2) + '-' + 
				            substring(mySiteCode, 5, 2) + '-' +
				            substring(mySiteCode, 7, 2)
			            )	as URLModecode
			
			
	            FROM 	V_PEOPLE_INFO_2_DIRECTORY	v,
			            REF_MODECODE				r
	            WHERE 	1=1
	
	
	
		            and 	(v.modecodeconc like '@MODE_CODE')
	
	 
	            AND 	(
				            v.status_code = 'A' 		OR 
				            v.status_code IS NULL
			            )
	            and		substring(v.modecodeconc, 1, 2) = r.modecode_1
	            and		substring(v.modecodeconc, 3, 2) = r.modecode_2
	            and		substring(v.modecodeconc, 5, 2) = r.modecode_3
	            and		substring(v.modecodeconc, 7, 2) = r.modecode_4
			
	            ORDER BY 	v.modecodeconc,
				            v.perlname, 
				            v.perfname

                ";

                sqlStr = sqlStr.Replace("@MODE_CODE", modeCodeWhere);

                sql = new Sql(sqlStr);

                peopleList = db.Query<PeopleByCity>(sql).ToList();
            }

            return peopleList;
        }


        public static List<PeopleInfo> GetPeople(string lname, string fname, string title, string phone, string email, string city, string state)
        {
            List<PeopleInfo> peopleList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;
            string where = "";

            if (false == string.IsNullOrWhiteSpace(lname) ||
                false == string.IsNullOrWhiteSpace(fname) ||
                false == string.IsNullOrWhiteSpace(title) ||
                false == string.IsNullOrWhiteSpace(phone) ||
                false == string.IsNullOrWhiteSpace(email) ||
                false == string.IsNullOrWhiteSpace(city) ||
                false == string.IsNullOrWhiteSpace(state))
            {
                if (false == string.IsNullOrWhiteSpace(lname))
                {
                    where += "REPLACE(REPLACE(REPLACE(perlname, '-',''), ' ',''), '.','') LIKE '%" + CleanSqlString(lname) + "%' AND ";
                }
                if (false == string.IsNullOrWhiteSpace(fname))
                {
                    where += "(perfname LIKE '%" + CleanSqlString(fname) + "%'  or percommonname like '%" + CleanSqlString(fname) + "%') AND ";
                }
                if (false == string.IsNullOrWhiteSpace(title))
                {
                    where += "(workingtitle LIKE '" + CleanSqlString(title) + "' OR officialtitle LIKE '" + CleanSqlString(title) + "') AND ";
                }
                if (false == string.IsNullOrWhiteSpace(phone))
                {
                    string phoneText = phone;
                    phoneText = phoneText.Replace("(", "").Replace(")", "");
                    phoneText = phoneText.Replace("-", "").Replace(" ", "");

                    where += "deskareacode + left(deskphone, 3)+ right(deskphone, 4) LIKE '%" + CleanSqlString(phoneText) + "%' AND ";
                }
                if (false == string.IsNullOrWhiteSpace(email))
                {
                    where += "email LIKE '%" + CleanSqlString(email) + "%' AND ";
                }
                if (false == string.IsNullOrWhiteSpace(city))
                {
                    where += "deskcity LIKE '%" + CleanSqlString(city) + "%' AND ";
                }
                if (false == string.IsNullOrWhiteSpace(state))
                {
                    where += "deskstate = '" + CleanSqlString(state) + "' AND ";
                }


                where += " (status_code = 'a' OR status_code is null)";

                sql = new Sql()
                 .Select("*")
                 .From("w_people_info")
                 .Where(where);

                peopleList = db.Query<PeopleInfo>(sql).ToList();
            }

            if (peopleList != null && peopleList.Count > 0)
            {
                peopleList = peopleList.OrderBy(p => p.LastName).ToList();
            }

            return peopleList;
        }


        public static List<string> GetAlphaList(string alpha)
        {
            List<string> alphaList = new List<string>();

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;
            string where = "";

            if (false == string.IsNullOrWhiteSpace(alpha))
            {
                if (alpha.Length == 2)
                {
                    alpha = alpha.Substring(0, 1);
                }

                where += "perlname LIKE '" + alpha + "%'";
                where += " AND (status_code = 'a' OR status_code IS NULL)";

                sql = new Sql()
                 .Select("DISTINCT '" + alpha.ToUpper() + "' + SUBSTRING(perlname, 2, 1) as chars")
                 .From("w_people_info")
                 .Where(where);

                alphaList = db.Query<string>(sql).ToList();
            }

            if (alphaList != null && alphaList.Count > 0)
            {
                alphaList = alphaList.OrderBy(p => p).ToList();
            }

            return alphaList;
        }



        public static List<PeopleInfo> GetPeopleAlpha(string alpha)
        {
            List<PeopleInfo> peopleList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;
            string where = "";

            if (false == string.IsNullOrWhiteSpace(alpha))
            {
                where += "REPLACE(REPLACE(REPLACE(perlname, '-',''), ' ',''), '.','') LIKE '" + CleanSqlString(alpha) + "%'";

                where += " AND (status_code = 'a' OR status_code is null)";

                sql = new Sql()
                 .Select("*")
                 .From("w_people_info")
                 .Where(where);

                peopleList = db.Query<PeopleInfo>(sql).ToList();
            }

            if (peopleList != null && peopleList.Count > 0)
            {
                peopleList = peopleList.OrderBy(p => p.LastName).ThenBy(x => x.FirstName).ToList();

                if (alpha.Length == 1)
                {
                    peopleList = GetPeopleAlpha(peopleList[0].LastName.Substring(0, 2));
                }
            }

            return peopleList;
        }


        public static List<PeopleInfo> GetPeopleByPosition(string jobSeriesCode)
        {
            List<PeopleInfo> peopleList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;
            string where = "";

            if (false == string.IsNullOrWhiteSpace(jobSeriesCode))
            {
                where += "SERIES_CODE = '" + jobSeriesCode + "'";

                where += " AND (status_code = 'a' OR status_code is null)";

                sql = new Sql()
                 .Select("*")
                 .From("w_people_info")
                 .Where(where);

                peopleList = db.Query<PeopleInfo>(sql).ToList();
            }

            if (peopleList != null && peopleList.Count > 0)
            {
                peopleList = peopleList.OrderBy(p => p.LastName).ThenBy(x => x.FirstName).ToList();
            }

            return peopleList;
        }


        public static PeopleInfo GetPerson(int personId)
        {
            PeopleInfo personInfo = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT perfname,EMP_ID,p_emp_id,POSITIONID,permname,perlname,percommonname,PERSONID,
			        MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_4,category,PositionTitle,officialtitle,SERIES_CODE,
			        workingtitle,email,IMAGEURL,Expertise,DESKPHONE,deskareacode,deskext,deskbldgabbr,ofcfax,ofcfaxareacode,
			        deskroomnum,DESKADDR1,deskaddr2,deskcity,deskstate,homepageurl,deskzip4,status_code,modecodeconc,mySiteCode 
			        FROM V_PEOPLE_INFO_2_DIRECTORY
			        WHERE personid = @personId
			        AND 1=1";

            personInfo = db.Query<PeopleInfo>(sql, new { personId = personId }).FirstOrDefault();

            return personInfo;
        }


        public static IPublishedContent GetPersonSite(int personId)
        {
            IPublishedContent node = Helpers.Nodes.GetNodeByPersonId(personId);

            return node;
        }


        private static string CleanSqlString(string str)
        {
            return str.Replace("'", "''").Replace(";", "");
        }
    }
}
