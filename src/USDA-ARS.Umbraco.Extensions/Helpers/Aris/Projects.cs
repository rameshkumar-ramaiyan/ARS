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


        public static List<UsdaProject> SearchProjects(string query, string type)
        {
            List<UsdaProject> projectList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT	p.accn_no, 
	                upper(substring(p.prj_title,1,1)) + lower(substring(p.prj_title, 
	                2, 
	                len(p.prj_title))) prj_title, 
	                modecodes.web_label
                FROM 		
	                W_CLEAN_PROJECTS 		p,		
	                v_locations MODECODES
                WHERE 	
	                (
		                1=0 
                    ";
            if (type == "all")
            {
                sql += " OR PRJ_TITLE LIKE '%'+ @query +'%'";
                sql += " OR APPROACH LIKE '%'+ @query +'%'";
                sql += " OR OBJECTIVE LIKE '%'+ @query +'%'";
                sql += " OR ProjectNumber LIKE '%'+ @query +'%' OR accn_no LIKE '%'+ @query +'%'";
            }
            else if (type == "title")
            {
                sql += " OR PRJ_TITLE LIKE '%'+ @query +'%'";
            }
            else if (type == "approach")
            {
                sql += " OR APPROACH LIKE '%'+ @query +'%'";
            }
            else if (type == "objective")
            {
                sql += " OR OBJECTIVE LIKE '%'+ @query +'%'";
            }
            else if (type == "project_number")
            {
                sql += " OR ProjectNumber LIKE '%'+ @query +'%' OR accn_no LIKE '%'+ @query +'%'";
            }

            sql += @"				
	            )
			
	            AND		(PRJ_TYPE <> 'J')
            
	            AND p.MODECODE_1 = modecodes.MODECODE_1
	            AND p.MODECODE_2 = modecodes.MODECODE_2
	            AND p.MODECODE_3 = modecodes.MODECODE_3
	            AND p.MODECODE_4 = modecodes.MODECODE_4
	            order by web_label";


            projectList = db.Query<UsdaProject>(sql, new { query = query }).ToList();

            if (projectList != null && projectList.Count > 0)
            {
                projectList = projectList.OrderByDescending(p => p.ProjectNumber).ToList();
            }


            return projectList;
        }



        public static List<ProjectTeam> GetProjectTeam(int accountNo)
        {
            List<ProjectTeam> teamList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT	PERSONID,PRIME_INDICATOR, PERLNAME,PERFNAME,PERCOMMONNAME
			        FROM 	V_PROJECT_TEAM
			        WHERE	ACCN_NO = @accountNo
			        ORDER BY PRIME_INDICATOR desc";

            teamList = db.Query<ProjectTeam>(sql, new { accountNo = accountNo }).ToList();

            return teamList;
        }


        public static List<string> GetProjectAnnualReportYears(int accountNo)
        {
            List<string> yearList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT	DISTINCT A421Q.FY
			        FROM	A421_QUESTIONS A421Q
			        WHERE 	A421Q.ACCN_NO = @accountNo
			        and		FY <> @currentYear		
			        ORDER BY fy desc";

            yearList = db.Query<string>(sql, new { accountNo = accountNo, currentYear = DateTime.Now.Year }).ToList();

            return yearList;
        }


        public static ProjectInfo GetProjectInfo(int accountNo)
        {
            ProjectInfo projectInfo = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT ACCN_NO, START_DATE, TERM_DATE, PROJECT_START, PROJECT_END, STATUS_CODE, PRJ_TITLE, AGREEMENT_CODE,
			        inst_code, FY, RESEARCH_FACILITIES, DURATION, modecode_1, modecode_2, modecode_3, modecode_4, ProjectNumber,
			        PRJ_NO_1, PRJ_NO_2, PRJ_NO_3, PRJ_NO_4, PRJ_TYPE, AGMNT_NO_1, AGMNT_NO_2, AGMNT_NO_3, AGMNT_NO_4, AGMNT_NO_5,
			        AGMNT_NO_6, SEQ_NO_425, COOPERATORS, BASIC_RESEARCH, APPLIED_RESEARCH, DEVELOPMENTAL_EFFORT, objective,
			        approach, NP_Number1, NP_Number2 
					
			        FROM V_CLEAN_PROJECTS_ALL
			        WHERE accn_no = @accountNo ";

            projectInfo = db.Query<ProjectInfo>(sql, new { accountNo = accountNo }).FirstOrDefault();

            return projectInfo;
        }


        public static bool HasProjectPublication(int accountNo)
        {
            bool hasPublications = false;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT accn_no
		        FROM gen_public_115s
		        WHERE ACCN_NO = @accountNo
		        ORDER BY journal_accpt_date desc
		        Option  (Robust Plan)";

            string count = db.Query<string>(sql, new { accountNo = accountNo }).FirstOrDefault();

            if (false == string.IsNullOrEmpty(count))
            {
                if (Convert.ToInt32(count) > 0)
                {
                    hasPublications = true;
                }
            }

            return hasPublications;
        }


        public static List<ProjectReport> GetProjectReport(int accountNo, string year)
        {
            List<ProjectReport> projectReportList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT 	R421Q.QUESTION_NO, RESPONSE, QUESTION, R421Q.VISUAL_QUESTION_NO,
			            A421Q.Q3_HEADER_TEXT			
	            FROM 	a421_QUESTIONS 		A421Q, 
			            REF_421_QUESTIONS 	R421Q
	            WHERE 	A421Q.accn_no 		= @accountNo
	            AND 	R421Q.QUESTION_NO 	= A421Q.QUESTION_NO 
	            AND 	R421Q.fy 			= @year
	            AND 	A421Q.FY 			= R421Q.fy
	
	            /*
	            AND 	R421Q.QUESTION_NO <> '31'
	            AND 	R421Q.QUESTION_NO <> '32'
	            */
	            -- don't include milestones questions for FY 2005 and 2006
	            AND		NOT (
					            A421Q.fy in ('2005', '2006') 		AND 
					            A421Q.question_no in ('31', '32')
				            )
				
	            -- don't include milestones questions for FY 2007/2008
	            AND 	NOT (
					 		
					            A421Q.fy in ('2007', '2008', '2009') AND			
					            A421Q.question_no = '20'
				            )
	
	            -- don't include question 2, 6, and 7 for FY 2010
	            AND 	NOT (
					            A421Q.fy in ('2010')				AND 
					            A421Q.question_no in ('20', '60', '70')
				            )

	            -- don't include question 2, 5, 6, and 7 for FY 2011
	            AND 	NOT (
					            A421Q.fy in ('2011')				AND 
					            A421Q.question_no in ('20', '50', '60', '70')
				            )

	            -- don't include question 2, 5, 6, and 7 for FY 2012
	            AND 	NOT (
					            A421Q.fy in ('2012')				AND 
					            A421Q.question_no in ('20', '50', '60', '70')
				            )
				
	            -- don't include question 2, 5, 6, and 7 for FY 2013
	            AND 	NOT (
					            A421Q.fy in ('2013')				AND 
					            A421Q.question_no in ('20', '50', '60', '70')
				            )

	            -- don't include question 5 for FY 2014 and 2015
	            AND 	NOT (
					            A421Q.fy in ('2014', '2015')				AND 
					            A421Q.question_no in ('50')
				            )
	
		            UNION ALL
			            select	40						as QUESTION_NO,
					            'None'					as RESPONSE,
					            'Accomplishments'		as QUESTION,
					            '4'						as VISUAL_QUESTION_NO,
					            ''						as Q3_HEADER_TEXT															
	
	            ORDER BY R421Q.QUESTION_NO";

            projectReportList = db.Query<ProjectReport>(sql, new { accountNo = accountNo, year = year }).ToList();

            return projectReportList;
        }


        public static List<ProjectAccomplishment> GetProjectAccomplishments(int accountNo, string year)
        {
            List<ProjectAccomplishment> projectAccompList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"select	accomp_no,
				            accomplishment
		            from	a421_accomplishments
		            where	accn_no	= @accountNo
		            and		fy		= @year
		            and		accomplishment IS NOT Null
		            order by accomp_no";

            projectAccompList = db.Query<ProjectAccomplishment>(sql, new { accountNo = accountNo, year = year }).ToList();

            return projectAccompList;
        }


        public static List<ProjectInfo> GetProjectsByPerson(string employeeId)
        {
            List<ProjectInfo> projectInfoList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT 	a.prj_title, b.prj_type, a.accn_no 
			        FROM 		
					        W_PERSON_PROJECTS	a, 			
						
					        W_CLEAN_PROJECTS 	b			
			        WHERE 	a.EMP_ID = @employeeId
			        AND 	a.accn_no = b.accn_no

			
			        AND		(PRJ_TYPE <> 'J')

			        ORDER BY prj_type";

            projectInfoList = db.Query<ProjectInfo>(sql, new { employeeId = employeeId }).ToList();

            return projectInfoList;
        }


        public static List<ProjectInfo> GetProjectsByCountry()
        {
            List<ProjectInfo> projectInfoList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT 	accn_no, c.country_code, country_desc, prj_title 
		            FROM 		
				            w_clean_projects p,				
				            ref_country c, 
				            ref_institute i
		            WHERE 	c.country_code <> 'US'
		            AND 	c.country_code = i.country_code
		            AND 	p.inst_code = i.inst_code
		            AND 	p.status_code ='a'
		            ORDER BY country_desc";

            projectInfoList = db.Query<ProjectInfo>(sql).ToList();

            return projectInfoList;
        }


        public static List<ProjectInfo> GetProjectsByCollaborations(string modeCode)
        {
            List<ProjectInfo> projectList = null;
            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"select	a.ACCN_NO,
				        p.PRJ_TITLE,
				        -- remove extra multiple spaces from RECIPIENT_NAME
				        -- reference: http://codejotter.wordpress.com/2010/03/12/sql-function-to-remove-extra-multiple-spaces-from-string
				        replace(replace(replace(r.RECIPIENT_NAME,' ','<>'),'><',''),'<>',' ') as RECIPIENT_NAME,
				        r.CITY_NAME, 
				        r.STATE_NAME,
				        r.country_description	
		        from 	AETS_Agreements a JOIN	REF_ETS_RECIPIENT r ON r.RECIPIENT_CODE = a.RECIPIENT_CODE
				        JOIN   V_CLEAN_PROJECTS p  ON a.ACCN_NO = p.ACCN_NO
		        where 	p.modecode_1 = 30 ";

            if (modeCodeArray[1] != "00")
            {
                sql += " and p.modecode_2 = " + modeCodeArray[1];
            }
            if (modeCodeArray[2] != "00")
            {
                sql += " and p.modecode_3 = " + modeCodeArray[2];
            }
            if (modeCodeArray[3] != "00")
            {
                sql += " and p.modecode_4 = " + modeCodeArray[3];
            }

            projectList = db.Query<ProjectInfo>(sql).ToList();

            return projectList;
        }


        public static List<ProjectSubject> GetProjectSubjects()
        {
            List<ProjectSubject> projectSubjectList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT	soi_code, long_desc
			        FROM 	ref_soi
			        WHERE 	soi_code IN (
									        SELECT 	DISTINCT soi_code 
									        FROM 				
											        W_CLEAN_PROJECTS 				a4m,	
											        a417_subject_of_investigation 	soi 
									        WHERE 	a4m.accn_no  = soi.accn_no
								        )
			        ORDER BY long_desc";

            projectSubjectList = db.Query<ProjectSubject>(sql).ToList();

            return projectSubjectList;
        }


        public static List<ProjectSubject> GetProjectSubjectsByModeCode(string modeCode)
        {
            List<ProjectSubject> projectSubjectList = null;
            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT	soi_code, long_desc
			        FROM	ref_soi
			        WHERE	soi_code IN 
						        (
							        SELECT DISTINCT soi_code 
							        FROM		
									        W_CLEAN_PROJECTS 				a4m,			
									        a417_subject_of_investigation 	soi 
							        WHERE	a4m.accn_no  = soi.accn_no";

            sql += " and a4m.modecode_1 = " + modeCodeArray[0];

            if (modeCodeArray[1] != "00")
            {
                sql += " and a4m.modecode_2 = " + modeCodeArray[1];
            }
            if (modeCodeArray[2] != "00")
            {
                sql += " and a4m.modecode_3 = " + modeCodeArray[2];
            }
            if (modeCodeArray[3] != "00")
            {
                sql += " and a4m.modecode_4 = " + modeCodeArray[3];
            }

            sql += @"			        )
			        ORDER BY long_desc";

            projectSubjectList = db.Query<ProjectSubject>(sql).ToList();

            return projectSubjectList;
        }


        public static List<ProjectInfo> GetProjectsBySubject(string modeCode, int soiCode)
        {
            List<ProjectInfo> projectList = null;

            var db = new Database("arisPublicWebDbDSN");
            string sql = "";

            if (false == string.IsNullOrEmpty(modeCode))
            {
                List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

                sql = @"SELECT 	distinct a4m.ACCN_NO, prj_title
			        FROM 			
					        W_CLEAN_PROJECTS				as a4m,				
					        a417_subject_of_investigation 	as soi
			        WHERE 	a4m.accn_no  = soi.accn_no";

                sql += " and a4m.modecode_1 = " + modeCodeArray[0];

                if (modeCodeArray[1] != "00")
                {
                    sql += " and a4m.modecode_2 = " + modeCodeArray[1];
                }
                if (modeCodeArray[2] != "00")
                {
                    sql += " and a4m.modecode_3 = " + modeCodeArray[2];
                }
                if (modeCodeArray[3] != "00")
                {
                    sql += " and a4m.modecode_4 = " + modeCodeArray[3];
                }

                sql += @" and soi.soi_code = @soiCode
			        ORDER BY prj_title";
            }
            else
            {
                sql = @"SELECT	distinct a4m.ACCN_NO, 
					        prj_title
			        FROM			
					        W_CLEAN_PROJECTS		a4m,		
					        a417_subject_of_investigation	soi
			        WHERE	a4m.accn_no  = soi.accn_no
			        AND		soi.soi_code =  @soiCode			
			        ORDER BY prj_title";
            }


            projectList = db.Query<ProjectInfo>(sql, new { soiCode = soiCode }).ToList();

            return projectList;
        }


        public static string ProjectTypeByCode(string code)
        {
            if (code == "A")
            {
                return "General Cooperative Agreement";
            }
            else if (code == "C")
            {
                return "Contract";
            }
            else if (code == "D")
            {
                return "Appropriated";
            }
            else if (code == "G")
            {
                return "Grant";
            }
            else if (code == "I")
            {
                return "Interagency Reimbursable Agreement";
            }
            else if (code == "M")
            {
                return "Memorandum of Understanding";
            }
            else if (code == "N")
            {
                return "Nonfunded Cooperative Agreement";
            }
            else if (code == "R")
            {
                return "Reimbursable";
            }
            else if (code == "S")
            {
                return "Specific Cooperative Agreement";
            }
            else if (code == "T")
            {
                return "Trust";
            }
            else if (code == "X")
            {
                return "Other";
            }
            else
            {
                return "Invalid Project Type";
            }
        }

    }
}
