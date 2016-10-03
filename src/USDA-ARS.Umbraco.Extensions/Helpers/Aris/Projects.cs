using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
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

			modeCode = ModeCodes.ModeCodeAddDashes(modeCode);

			List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

			if (modeCodeArray != null && modeCodeArray.Count == 4)
			{
				string cacheKey = "ListProjects:" + modeCode;
				int cacheUpdateInMinutes = 720;
				ObjectCache cache = MemoryCache.Default;

				if (modeCode.EndsWith("00-00"))
				{
					projectList = cache.Get(cacheKey) as List<UsdaProject>;
				}

				if (projectList == null)
				{
					var db = new Database("arisPublicWebDbDSN");

					Sql sql = null;

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

					if (modeCode.EndsWith("00-00"))
					{
						CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
						cache.Add(cacheKey, projectList, policy);
					}
				}
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

			if (false == string.IsNullOrWhiteSpace(query))
			{
				query = query.Replace("'", "");
			}

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


		public static List<string> GetProjectAnnualReportYears(int accountNo, int currentYear = 2050)
		{
			List<string> yearList = null;

			if (currentYear >= 2050)
			{
				currentYear = DateTime.Now.Year;
			}

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT	DISTINCT A421Q.FY
			        FROM	A421_QUESTIONS A421Q
			        WHERE 	A421Q.ACCN_NO = @accountNo
			        and		FY <= @currentYear		
			        ORDER BY fy desc";

			yearList = db.Query<string>(sql, new { accountNo = accountNo, currentYear = currentYear }).ToList();

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

		/// <summary>
		/// Gets national projects by person who is a leader on
		/// </summary>
		/// <param name="employeeId"></param>
		/// <returns></returns>
		public static List<ProjectInfo> GetNationalProgramLeaderByPerson(int personId)
		{
			List<ProjectInfo> projectList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"select	p.accn_no, 
			                     p.prj_title
	                     from	V_PEOPLE_INFO_2_DIRECTORY	d,
			                     ref_signature				s,
			                     a416_responsible_team		t,
			                     v_clean_projects			p
	                     where	1=1
	                     and		d.personid = @personId
	                     and		s.emp_id = d.emp_id
	                     and		s.status_code = 'A'
	                     and		t.RESP_CODE = s.sig_code
	                     and		t.person_type = 'L'
	                     and		p.accn_no = t.accn_no
	                     and		p.prj_type = 'D'";

			projectList = db.Query<ProjectInfo>(sql, new { personId = personId }).ToList();

			return projectList;
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

			string cacheKey = "GetProjectsByCollaborations:" + modeCode;
			int cacheUpdateInMinutes = 720;

			ObjectCache cache = MemoryCache.Default;

			projectList = cache.Get(cacheKey) as List<ProjectInfo>;

			if (projectList == null)
			{
				List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

				if (modeCodeArray != null && modeCodeArray.Count == 4)
				{
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
						where 	p.modecode_1 = " + modeCodeArray[0];

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

					CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
					cache.Add(cacheKey, projectList, policy);
				}
			}

			return projectList;
		}


		public static List<ProjectInfo> GetProjectAnnualReportList(string npCode, string sort, int year = 2050)
		{
			List<ProjectInfo> projectList = null;

			int npCodeInt = 0;

			if (int.TryParse(npCode, out npCodeInt))
			{
				var db = new Database("arisPublicWebDbDSN");

				string sql = @"SELECT 	distinct p.prj_title, p.accn_no, p.prj_type project_type, q.fy,
					            modecodes.Web_Label, city, rtrim(state_CODE) stateabbr,
					            modecodes.modecode_1, modecodes.modecode_2, modecodes.modecode_3, modecodes.modecode_4
			            FROM 			
					            w_clean_projects		p,				
					            a421_questions 			q, 
					            a416_national_program 	np, 
					            v_locations MODECODES
			            WHERE 	q.accn_no = p.accn_no
			            AND 	q.fy >=  datepart(yyyy, getdate()) -5
															AND  q.fy <= @year
			            AND 	np.accn_no = p.accn_no
			            AND 	np_code = @npCode
			            AND 	np_percent >= 30
			            AND 	p.MODECODE_1 = modecodes.MODECODE_1
			            AND 	p.MODECODE_2 = modecodes.MODECODE_2
			            AND 	p.MODECODE_3 = modecodes.MODECODE_3
			            AND 	p.MODECODE_4 = modecodes.MODECODE_4
                        
                        ";

				if (false == string.IsNullOrWhiteSpace(sort) && sort.ToLower() == "projecttype")
				{
					sql += "ORDER BY p.prj_type, modecodes.modecode_1, modecodes.modecode_2, modecodes.modecode_3, modecodes.modecode_4";
				}
				else
				{
					sql += "ORDER BY modecodes.modecode_1, modecodes.modecode_2, modecodes.modecode_3, modecodes.modecode_4, p.prj_type";
				}


				projectList = db.Query<ProjectInfo>(sql, new { npCode = npCode, year = year }).ToList();
			}

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

			if (modeCodeArray != null && modeCodeArray.Count == 4)
			{
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
			}

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


		public static List<ProjectInfo> GetProjectsByStateAndProgram(string state, string npCode)
		{
			List<ProjectInfo> projectInfoList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT	
				            case
					            when modecodes.modecode_4_desc = '' OR modecodes.modecode_4_desc IS NULL			
						            then modecodes.modecode_3_desc
					            when modecodes.modecode_4_desc = 'Office of the Director'
						            then modecodes.modecode_3_desc
					            else modecodes.modecode_4_desc										
				            end as modecode_desc,
				
				            A4M.PRJ_TITLE, 
				            A4M.ACCN_NO,
								
				            (
					            right('00' + cast(MODECODES.modecode_1 as varchar(2)), 2) + '-' +
					            right('00' + cast(MODECODES.modecode_2 as varchar(2)), 2) + '-' +
					            right('00' + cast(MODECODES.modecode_3 as varchar(2)), 2) + '-' +
					            right('00' + cast(MODECODES.modecode_4 as varchar(2)), 2) 
				            ) as modecode

		            FROM	V_CLEAN_PROJECTS		A4M, 
				            REF_MODECODE 			MODECODES, 
				            A416_NATIONAL_PROGRAM 	A4NP

		            where	STATE_CODE = @state AND A4NP.np_CODE = @npCode

		            and a4np.np_type = 'n'
		            AND A4NP.ACCN_NO = A4M.ACCN_NO
		
		            and A4M.MODECODE_1 = modecodes.MODECODE_1
		            and A4M.MODECODE_2 = modecodes.MODECODE_2
		            and A4M.MODECODE_3 = modecodes.MODECODE_3
		            and A4M.MODECODE_4 = modecodes.MODECODE_4
		
		            and modecodes.MODECODE_3_DESC <> ''	-- remove xx-00-00-00 and xx-xx-00-00  (i.e.02-00-00-00 and 02-06-00-00)	
		
		            order by	modecode desc,
						
					            modecode_desc,							
					            A4M.ACCN_NO	";

			projectInfoList = db.Query<ProjectInfo>(sql, new { state = state, npCode = npCode }).ToList();

			return projectInfoList;
		}


		public static List<ProjectInfo> GetProjectsByProgramFilter(string npCode, string projectStatus = "A",
										string peopleList = "", string projectType = "", string location = "", string orderBy = "")
		{
			List<ProjectInfo> projectInfoList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT 	projects.accn_no, projects.prj_title, projects.PRJ_TYPE project_type, 
					                modecodes.Web_Label,city, rtrim(state_CODE) stateabbr, perfname, perlname, 
					                (city + ',' + ' ' + state_code) AS location
			                FROM 									
					                w_people_info			v_people_info,				
					                v_locations 			MODECODES, 
					                A416_NATIONAL_PROGRAM 	A4NP,
					                W_PERSON_PROJECTS_ALL	V_PERSON_PROJECTS,
					                W_CLEAN_PROJECTS_ALL	PROJECTS
					
					
			                WHERE 	NP_CODE = @npCode
			                AND		projects.status_code = @projectStatus							
			                AND 	A4NP.ACCN_NO = PROJECTS.ACCN_NO
			                AND 	projects.MODECODE_1 = modecodes.MODECODE_1
			                AND 	projects.MODECODE_2 = modecodes.MODECODE_2
			                AND 	projects.MODECODE_3 = modecodes.MODECODE_3
			                AND 	projects.MODECODE_4 = modecodes.MODECODE_4
			                AND 	projects.MODECODE_2 <> '01'
			                AND 	MODECODES.STATUS_CODE = 'a'
			                AND 	left(modecodes.MODECODE_1, 2) > 05
			                AND 	projects.accn_no = v_person_projects.accn_no
			                AND 	v_person_projects.emp_id = v_people_info.emp_id 
			                AND 	v_person_projects.emp_id IS NOT NULL
			                AND 	v_people_info.status_code = 'A'
						
			
			                AND		(PRJ_TYPE <> 'J')";

			if (false == string.IsNullOrEmpty(peopleList))
			{
				sql += " AND v_people_info.personid IN (" + Utilities.Strings.CleanSqlString(peopleList) + ") ";
			}
			if (false == string.IsNullOrEmpty(projectType))
			{
				sql += " AND projects.prj_type = '" + Utilities.Strings.CleanSqlString(projectType) + "' ";
			}
			if (false == string.IsNullOrEmpty(projectType))
			{
				sql += " AND (city + ',' + ' ' + state_code) = '" + Utilities.Strings.CleanSqlString(location) + "' ";
			}
			sql += " ORDER BY ";

			if (false == string.IsNullOrEmpty(orderBy))
			{
				sql += orderBy + ", ";
			}
			sql += "modecodes.modecode_1, modecodes.modecode_2, modecodes.modecode_3, modecodes.modecode_4";


			projectInfoList = db.Query<ProjectInfo>(sql, new { npCode = npCode, projectStatus = projectStatus }).ToList();

			return projectInfoList;
		}


		public static List<CityState> GetLocationsByProject(string npCode, string projectStatus = "A")
		{
			List<CityState> cityStateList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT	DISTINCT MODECODES.city, 
		                  MODECODES.state_code, 
		                  (MODECODES.city + ',' + ' ' + MODECODES.state_code) AS location 
                  FROM		
		                  w_clean_projects_all	PROJECTS,			
		                  v_locations				MODECODES
			
                  WHERE 	PROJECTS.ACCN_NO in 
					                  (
						                  select	distinct accn_NO
						                  from	A416_national_program
						                  where 	NP_code = @npCode
					                  )
	                  AND		projects.STATUS_CODE = @projectStatus
	                  AND		projects.MODECODE_1 = modecodes.MODECODE_1
	                  AND		projects.MODECODE_2 = modecodes.MODECODE_2
	                  AND		projects.MODECODE_3 = modecodes.MODECODE_3
	                  AND		projects.MODECODE_4 = modecodes.MODECODE_4
	                  AND		projects.MODECODE_2 <> '01'
	                  AND		MODECODES.STATUS_CODE = 'a'
	                  AND		left(modecodes.MODECODE_1, 2) > 05
                  ORDER BY location";

			cityStateList = db.Query<CityState>(sql, new { npCode = npCode, projectStatus = projectStatus }).ToList();

			return cityStateList;
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
