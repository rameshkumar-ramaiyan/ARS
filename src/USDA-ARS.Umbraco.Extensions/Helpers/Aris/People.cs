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
    }
}
