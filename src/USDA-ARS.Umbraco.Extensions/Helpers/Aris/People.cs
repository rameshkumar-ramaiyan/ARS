using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using System.Text.RegularExpressions;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
	public class People
	{
		public static List<PeopleInfo> GetPeople(string modeCode)
		{
			List<PeopleInfo> peopleList = null;

			List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

			if (modeCodeArray != null && modeCodeArray.Any())
			{
				var db = new Database("arisPublicWebDbDSN");

				Sql sql = null;

				if (modeCodeArray != null && modeCodeArray.Count == 4)
				{
					string where = "MODECODE_1 = '" + modeCodeArray[0] + "' AND ";
					where += "MODECODE_2 = '" + modeCodeArray[1] + "' AND ";
					where += "MODECODE_3 = '" + modeCodeArray[2] + "' AND ";
					where += "MODECODE_4 = '" + modeCodeArray[3] + "'";

					if (modeCodeArray[0] == "00")
					{
						where = "";
						where += "MODECODE_1 = '00'";
						for (int i = 1; i < 10; i++)
						{
							where += " OR MODECODE_1 = '0" + i + "'";
						}
					}


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
			}

			return peopleList;
		}


		public static List<PeopleByCity> GetPeopleByCity(string modeCode)
		{
			List<PeopleByCity> peopleList = null;

			List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

			if (modeCodeArray != null && modeCodeArray.Any())
			{
				var db = new Database("arisPublicWebDbDSN");

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


					if (modeCodeArray[0] == "00")
					{
						modeCodeWhere = "0";
					}


					string sql = @"SELECT	v.mySiteCode,
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
	
					AND 	(v.modecodeconc like @modeCodeWhere + '%')
	
	 
					AND 	(v.status_code = 'A' OR v.status_code IS NULL
					)
					and		substring(v.modecodeconc, 1, 2) = r.modecode_1
					and		substring(v.modecodeconc, 3, 2) = r.modecode_2
					and		substring(v.modecodeconc, 5, 2) = r.modecode_3
					and		substring(v.modecodeconc, 7, 2) = r.modecode_4
			
					ORDER BY 	v.modecodeconc,
								v.perlname, 
								v.perfname
												";

					peopleList = db.Query<PeopleByCity>(sql, new { modeCodeWhere = modeCodeWhere }).ToList();

					if (peopleList != null && peopleList.Any())
					{
						List<IPublishedContent> modeCodeList = Nodes.GetNodesListOfModeCodes();

						foreach (PeopleByCity peopleByCity in peopleList)
						{
							string modeCodeTest = ModeCodes.ModeCodeAddDashes(peopleByCity.ModeCodeConcat);

							if (false == string.IsNullOrEmpty(modeCodeTest))
							{
								IPublishedContent foundLocation = modeCodeList.Where(p => false == string.IsNullOrEmpty(p.GetPropertyValue<string>("modeCode")) &&
											p.GetPropertyValue<string>("modeCode") == modeCodeTest).FirstOrDefault();

								if (foundLocation != null)
								{
									peopleByCity.ModeCode = modeCodeTest;
									peopleByCity.SiteLabel = foundLocation.Name;
								}
							}
						}
					}
				}
			}

			return peopleList;
		}


		public static List<PeopleInfo> GetPeopleByNationalProgram(string modeCode)
		{
			List<PeopleInfo> peopleList = null;

			List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

			if (modeCodeArray != null && modeCodeArray.Any())
			{
				modeCode = Helpers.ModeCodes.ModeCodeNoDashes(modeCode);

				var db = new Database("arisPublicWebDbDSN");
				string sql = @"select modecodeconc, personid, perlname, perfname, permname, percommonname, EMail, DeskPhone, deskareacode, officialtitle, workingtitle
			                        from V_PEOPLE_INFO_2
			                        where modecodeconc = @modeCode
			                        and (status_code = 'a' or status_code is null)
			                        order by perlname, perfname ";

				peopleList = db.Query<PeopleInfo>(sql, new { modeCode = modeCode }).ToList();
			}

			return peopleList;
		}


		public static List<PeopleInfo> GetPeople(string lname, string fname, string title, string phone, string email, string city, string state)
		{
			List<PeopleInfo> peopleList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = null;
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
					where += "REPLACE(REPLACE(REPLACE(perlname, '-',''), ' ',''), '.','') LIKE '%'+ @lname +'%' AND ";
				}
				if (false == string.IsNullOrWhiteSpace(fname))
				{
					where += "(perfname LIKE '%'+ @fname +'%'  or percommonname like '%'+ @fname +'%') AND ";
				}
				if (false == string.IsNullOrWhiteSpace(title))
				{
					where += "(workingtitle LIKE '%'+ @title +'%' OR officialtitle LIKE '%'+ @title +'%') AND ";
				}
				if (false == string.IsNullOrWhiteSpace(phone))
				{
					string phoneText = phone;
					phoneText = phoneText.Replace("(", "").Replace(")", "");
					phoneText = phoneText.Replace("-", "").Replace(" ", "");

					phone = phoneText;

					where += "deskareacode + left(deskphone, 3)+ right(deskphone, 4) LIKE '%'+ @phone +'%' AND ";
				}
				if (false == string.IsNullOrWhiteSpace(email))
				{
					where += "email LIKE '%'+ @email +'%' AND ";
				}
				if (false == string.IsNullOrWhiteSpace(city))
				{
					where += "deskcity LIKE '%'+ @city +'%' AND ";
				}
				if (false == string.IsNullOrWhiteSpace(state))
				{
					where += "deskstate = @state AND ";
				}


				where += " (status_code = 'a' OR status_code is null)";

				sql = "SELECT * FROM v_people_info WHERE " + where;

				peopleList = db.Query<PeopleInfo>(sql, new { lname = lname, fname = fname, title = title, phone = phone, email = email, city = city, state = state }).ToList();
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

			if (false == string.IsNullOrWhiteSpace(alpha))
			{
				if (Regex.IsMatch(alpha, @"^[a-zA-Z]{1,2}$"))
				{
					var db = new Database("arisPublicWebDbDSN");

					Sql sql = null;
					string where = "";

					alpha = alpha.Trim();

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
						alphaList = alphaList.Where(p => p.Trim().Length >= 2).ToList();

						alphaList = alphaList.OrderBy(p => p).ToList();
					}
				}
			}

			return alphaList;
		}



		public static List<PeopleInfo> GetPeopleAlpha(string alpha)
		{
			List<PeopleInfo> peopleList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"select personid, perlname, perfname, permname, percommonname,
                            EMail, DeskPhone, deskareacode, officialtitle
                            from v_people_info
                            where  left(perlname, 2) = @alpha
                            and(status_code = 'a' or status_code is null)
                            order by perlname , perfname asc
                            ";

			if (false == string.IsNullOrWhiteSpace(alpha))
			{
				if (alpha.Length == 1)
				{
					sql = @"select personid, perlname, perfname, permname, percommonname,
                            EMail, DeskPhone, deskareacode, officialtitle
                            from v_people_info
                            where  left(perlname, 1) = @alpha
                            and(status_code = 'a' or status_code is null)
                            order by perlname , perfname asc
                            ";

					peopleList = db.Query<PeopleInfo>(sql, new { alpha = alpha }).ToList();

					if (peopleList != null && peopleList.Count > 0)
					{
						if (peopleList[0].LastName.Substring(0, 2).Trim().Length == 1 && peopleList.Count > 1)
						{
							peopleList = GetPeopleAlpha(peopleList[1].LastName.Substring(0, 2));
						}
						else
						{
							peopleList = GetPeopleAlpha(peopleList[0].LastName.Substring(0, 2));
						}
					}
				}
				else
				{
					peopleList = db.Query<PeopleInfo>(sql, new { alpha = alpha }).ToList();
				}
			}

			if (peopleList != null && peopleList.Count > 0)
			{
				peopleList = peopleList.OrderBy(p => p.LastName).ThenBy(x => x.FirstName).ToList();
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
				 .From("v_people_info")
				 .Where(where);

				peopleList = db.Query<PeopleInfo>(sql).ToList();
			}

			if (peopleList != null && peopleList.Count > 0)
			{
				peopleList = peopleList.OrderBy(p => p.LastName).ThenBy(x => x.FirstName).ToList();
			}

			return peopleList;
		}


		public static List<PeopleInfo> GetPeopleByPublication(int seqNo115)
		{
			List<PeopleInfo> peopleList = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT     a.EMP_ID, a.EMPLOYER, ISNULL(p.PerLName, ISNULL(r.LAST_NAME, a.LAST_NAME)) AS perlname, ISNULL(p.PerFName, ISNULL(r.FIRST_NAME, a.FIRST_NAME))
                                AS perfname, p.PerCommonName, p.PersonID
                            FROM         REF_PERSONEL AS r RIGHT OUTER JOIN
                                v_AH115_Authors AS a ON r.EMP_ID = a.EMP_ID LEFT OUTER JOIN
                                aris_public_web.dbo.People AS p ON a.EMP_ID = p.EmpID
                            WHERE     (a.SEQ_NO_115 = @seqNo115)
                            ORDER BY a.AUTHORSHIP";

			peopleList = db.Query<PeopleInfo>(sql, new { seqNo115 = seqNo115 }).ToList();

			return peopleList;
		}


		public static List<PeopleInfo> GetPeopleByProject(string npCode, string projectStatus = "A")
		{
			List<PeopleInfo> peopleList = null;

			int npCodeInt = 0;

			if (int.TryParse(npCode, out npCodeInt))
			{
				var db = new Database("arisPublicWebDbDSN");

				string sql = @"SELECT DISTINCT v_people_info.emp_id, v_people_info.personid, v_people_info.perlname, 
					        (v_people_info.perlname + ',' + ' ' + v_people_info.perfname) AS fullName
                    FROM A416_national_program   A4NP,
					        w_person_projects_all v_person_projects,
                            w_people_info           v_people_info,
					        w_clean_projects_all PROJECTS
                    WHERE A4NP.NP_CODE = @npCode
                    AND A4NP.ACCN_NO = PROJECTS.ACCN_NO
                    AND projects.MODECODE_2 <> '01'
                    AND projects.STATUS_CODE = @projectStatus -- chg - 02
                    AND left(projects.MODECODE_1, 2) > 05
                    AND projects.accn_no = v_person_projects.accn_no
                    AND v_person_projects.emp_id = v_people_info.emp_id
                    AND v_person_projects.emp_id IS NOT NULL
                    AND v_people_info.status_code = 'A'
                    ORDER BY perlname";

				peopleList = db.Query<PeopleInfo>(sql, new { npCode = npCode, projectStatus = projectStatus }).ToList();
			}

			return peopleList;


		}


		public static PeopleInfo GetPerson(int personId)
		{
			PeopleInfo personInfo = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"SELECT perfname,EMP_ID,p_emp_id,permname,perlname,percommonname,PERSONID,
			        MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_4,category,officialtitle,SERIES_CODE,
			        workingtitle,email,IMAGEURL,DESKPHONE,deskareacode,deskext,deskbldgabbr,ofcfax,ofcfaxareacode,
			        deskroomnum,DESKADDR1,deskaddr2,deskcity,deskstate,homepageurl,deskzip4,status_code,modecodeconc,mySiteCode 
			        FROM V_PEOPLE_INFO_2_DIRECTORY
			        WHERE personid = @personId
			        AND 1=1";

			personInfo = db.Query<PeopleInfo>(sql, new { personId = personId }).FirstOrDefault();

			return personInfo;
		}


		public static PeopleInfo GetPersonByEmployeeId(string empId)
		{
			PeopleInfo personInfo = null;

			var db = new Database("arisPublicWebDbDSN");

			string sql = @"Select * 
	                    from V_PEOPLE_INFO 
	                    where EMP_ID = @empId";

			personInfo = db.Query<PeopleInfo>(sql, new { empId = empId }).FirstOrDefault();

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
