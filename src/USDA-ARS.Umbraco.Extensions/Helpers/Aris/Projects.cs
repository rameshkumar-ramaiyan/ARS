using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class Projects
    {
        public static List<UsdaProject> ListProjects(string modeCode, int count = -1)
        {
            List<UsdaProject> projectList = null;

            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            if (modeCodeArray != null && modeCodeArray.Count == 4)
            {
                string where = "MODECODE_1 = '" + modeCodeArray[0] + "' AND ";

                if (modeCodeArray[1] != "00")
                {
                    where += "MODECODE_2 = '" + modeCodeArray[1] + "' AND ";
                }
                if (modeCodeArray[2] != "00")
                {
                    where += "MODECODE_3 = '" + modeCodeArray[2] + "' AND ";
                }
                if (modeCodeArray[3] != "00")
                {
                    where += "MODECODE_4 = '" + modeCodeArray[3] + "' AND ";
                }

                if (where.EndsWith(" AND "))
                {
                    where = where.Substring(0, where.LastIndexOf(" AND "));
                }

                where += " AND status_code = 'a' AND prj_type = 'd'";

                sql = new Sql()
                 .Select("*")
                 .From("V_CLEAN_PROJECTS")
                 .Where(where);

                projectList = db.Query<UsdaProject>(sql).ToList();
            }

            if (projectList != null && projectList.Count > 0)
            {
                projectList = projectList.OrderBy(p => p.Title).ToList();

                if (count > 0)
                {
                    projectList = projectList.Take(count).ToList();
                }
            }


            return projectList;
        }
    }
}
