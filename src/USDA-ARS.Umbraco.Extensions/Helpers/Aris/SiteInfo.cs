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
                        this.AdminDetails += "(" + peopleInfo.PhoneAreaCode + ") " + peopleInfo.Phone.Substring(0, 3) + "-" + peopleInfo.Phone.Substring(3,4) + "<br />\r\n";
                        this.AdminDetails += refModeCode.RsAddress1 + "<br />\r\n";
                        this.AdminDetails += refModeCode.RsAddress2 + "<br />\r\n";
                        this.AdminDetails += Utilities.Strings.UppercaseFirst(peopleInfo.City) + " " + refModeCode.RsStateCode + " " + refModeCode.RsPostalCode + "<br />\r\n";
                    }
                }
            }



        }

    }
}
