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
	public class ProjectPrograms
	{
		public static List<ProjectProgram> GetProjectPrograms(string modeCode, bool appropriatedOnly = false)
		{
			List<ProjectProgram> projectProgramList = null;

			modeCode = ModeCodes.ModeCodeAddDashes(modeCode);

			List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

			if (modeCodeArray != null && modeCodeArray.Any())
			{
				string cacheKey = "GetProjectPrograms:" + modeCode;
				int cacheUpdateInMinutes = 720;
				ObjectCache cache = MemoryCache.Default;

				if (modeCode.EndsWith("00-00"))
				{
					projectProgramList = cache.Get(cacheKey) as List<ProjectProgram>;
				}

				if (projectProgramList == null)
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

					sql += @" order by    A4NP.NP_CODE,
	                    (case when p.prj_type = 'D' then 1
	                          else 2
                        end),
	                    short_desc,
	                    prj_type,
	                    prj_title
                ";


					projectProgramList = db.Query<ProjectProgram>(sql).ToList();

					if (modeCode.EndsWith("00-00"))
					{
						CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
						cache.Add(cacheKey, projectProgramList, policy);
					}
				}
			}

			// if Appropriate Only...
			if (projectProgramList != null && projectProgramList.Any() && true == appropriatedOnly)
			{
				projectProgramList = projectProgramList.Where(p => p.ProjectType == "d").ToList();
			}

			return projectProgramList;
		}

		public static List<ProjectProgram> GetProjectProgramsByNpCode(string npCode, string projectStatus, List<int> personIdList = null, string projectType = "", string location = "N", string orderBy = "L")
		{
			List<ProjectProgram> projectProgramList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT 	projects.accn_no, projects.prj_title, projects.PRJ_TYPE prj_type, 
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

			if (personIdList != null && personIdList.Any())
			{
				sql += " AND v_people_info.personid IN (" + string.Join(",", personIdList) + ")\r\n";
			}

			if (false == string.IsNullOrWhiteSpace(projectType))
			{
				sql += " AND projects.prj_type = @projectType \r\n";
			}

			if (false == string.IsNullOrWhiteSpace(location))
			{
				sql += " AND (city + ',' + ' ' + state_code) = @location \r\n";
			}

			if (false == string.IsNullOrWhiteSpace(orderBy) && orderBy == "P")
			{
				sql += " ORDER BY projects.prj_type, modecodes.modecode_1, modecodes.modecode_2, modecodes.modecode_3, modecodes.modecode_4  \r\n";
			}
			else
			{
				sql += " ORDER BY modecodes.modecode_1, modecodes.modecode_2,modecodes.modecode_3, modecodes.modecode_4, projects.prj_type, projects.accn_no \r\n";
			}


			projectProgramList = db.Query<ProjectProgram>(sql, new { npCode = npCode, projectStatus = projectStatus, projectType = projectType, location = location }).ToList();

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
