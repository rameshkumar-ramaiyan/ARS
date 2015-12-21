using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    where += "(perfname like '%" + CleanSqlString(fname) + "%'  or percommonname like '%" + CleanSqlString(fname) + "%') AND ";
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
                    where += "email = '%" + CleanSqlString(email) + "%' AND ";
                }
                if (false == string.IsNullOrWhiteSpace(city))
                {
                    where += "deskcity = '%" + CleanSqlString(city) + "%' AND ";
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

                where += "perlname LIKE '"+ alpha +"%'";
                where += " AND (status_code = 'a' OR status_code IS NULL)";

                sql = new Sql()
                 .Select("DISTINCT '"+ alpha.ToUpper() +"' + SUBSTRING(perlname, 2, 1) as chars")
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
                peopleList = peopleList.OrderBy(p => p.LastName).ToList();

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
                where += "SERIES_CODE = '"+ jobSeriesCode + "'";

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

            Sql sql = null;

            string where = "PERSONID = '" + personId + "'";

            sql = new Sql()
             .Select("*")
             .From("w_people_info")
             .Where(where);

            personInfo = db.Query<PeopleInfo>(sql).FirstOrDefault();

            return personInfo;
        }


        private static string CleanSqlString(string str)
        {
            return str.Replace("'", "''").Replace(";", "");
        }
    }
}
