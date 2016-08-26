using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
   public class ProjectPrograms
   {
      public static List<ProjectProgram> GetProjectPrograms(string modeCode, bool appropriatedOnly = false)
      {
         List<Models.Aris.ProjectProgram> projectProgramList = null;
         List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

         if (modeCodeArray != null && modeCodeArray.Any())
         {
            var db = new Database("arisPublicWebDbDSN");

            string sql = "";

            sql += @"
            select
                    A4NP.NP_CODE, 
			        RNP.short_desc, 
			        p.accn_no, p.prj_title, p.prj_type
            from    A416_NATIONAL_PROGRAM A4NP,
                    REF_NATIONAL_PROGRAM    RNP,
			        v_clean_projects P
            where RNP.NP_CODE = A4NP.NP_CODE
            and P.accn_no = A4NP.accn_no
            and P.status_code = 'a'";

            sql += " and modecode_1 = " + modeCodeArray[0];

            if (modeCodeArray[1] != "00")
            {
               sql += " and modecode_2 = " + modeCodeArray[1];
            }
            if (modeCodeArray[2] != "00")
            {
               sql += " and modecode_3 = " + modeCodeArray[2];
            }
            if (modeCodeArray[3] != "00")
            {
               sql += " and modecode_4 = " + modeCodeArray[3];
            }

            sql += " and p.prj_type <> 'J'";

            if (true == appropriatedOnly)
            {
               sql += " and p.prj_type = 'd'";
            }

            sql += @" order by    A4NP.NP_CODE,
	                    (case when p.prj_type = 'D' then 1
	                          else 2
                        end),
	                    short_desc,
	                    prj_type,
	                    prj_title
                ";


            projectProgramList = db.Query<Models.Aris.ProjectProgram>(sql).ToList();
         }

         return projectProgramList;
      }

      public static List<ProjectProgram> GetRelatedPrograms(int accountNo)
      {
         List<Models.Aris.ProjectProgram> projectProgramList = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT A4NP.NP_CODE,SHORT_DESC 
		            FROM A416_NATIONAL_PROGRAM A4NP, REF_NATIONAL_PROGRAM RNP
		            WHERE accn_no = @accountNo
		            AND RNP.NP_CODE = A4NP.NP_CODE";

         projectProgramList = db.Query<Models.Aris.ProjectProgram>(sql, new { accountNo = accountNo }).ToList();

         return projectProgramList;
      }
   }
}
