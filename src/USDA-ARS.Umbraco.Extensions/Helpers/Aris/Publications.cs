using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class Publications
    {
        public static List<UsdaPublication> ListPublications(string modeCode, int count = -1)
        {
            List<UsdaPublication> publicationList = null;

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

                where += " AND LEFT(DATEPART(yyyy,journal_accpt_date), 4) = '" + DateTime.Now.Year + "'";

                sql = new Sql()
                 .Select("*")
                 .From("gen_public_115s")
                 .Where(where);

                publicationList = db.Query<UsdaPublication>(sql).ToList();
            }



            List<ModeCodeNew> modeCodeList = Helpers.Aris.ModeCodesNew.GetOldModeCode(modeCode);

            foreach (ModeCodeNew modeCodeItem in modeCodeList)
            {
                Sql sql2 = null;
                List<UsdaPublication> publicationList2 = null;

                List<string> modeCodeArray2 = Helpers.ModeCodes.ModeCodeArray(modeCodeItem.ModecodeOld);

                if (modeCodeArray2 != null && modeCodeArray2.Count == 4)
                {
                    string where = "MODECODE_1 = '" + modeCodeArray2[0] + "' AND ";

                    if (modeCodeArray2[1] != "00")
                    {
                        where += "MODECODE_2 = '" + modeCodeArray2[1] + "' AND ";
                    }
                    if (modeCodeArray2[2] != "00")
                    {
                        where += "MODECODE_3 = '" + modeCodeArray2[2] + "' AND ";
                    }
                    if (modeCodeArray2[3] != "00")
                    {
                        where += "MODECODE_4 = '" + modeCodeArray2[3] + "' AND ";
                    }

                    if (where.EndsWith(" AND "))
                    {
                        where = where.Substring(0, where.LastIndexOf(" AND "));
                    }

                    where += " AND LEFT(DATEPART(yyyy,journal_accpt_date), 4) = '" + DateTime.Now.Year + "'";

                    sql2 = new Sql()
                     .Select("*")
                     .From("gen_public_115s")
                     .Where(where);

                    publicationList2 = db.Query<UsdaPublication>(sql2).ToList();

                    if (publicationList == null)
                    {
                        publicationList = publicationList2;
                    }
                    else
                    {
                        publicationList.AddRange(publicationList2);
                    }
                }

            }

            if (publicationList != null && publicationList.Count > 0)
            {
                publicationList = publicationList.OrderByDescending(p => p.JournalAcceptDate).ToList();

                if (count > 0)
                {
                    publicationList = publicationList.Take(count).ToList();
                }
            }


            return publicationList;
        }


        public static SearchPublicationView SearchPublicationsMain(string query = "", string type = "", int count = 0, int skip = 0)
        {
            SearchPublicationView publicationListView = new SearchPublicationView();
            List<SearchPublication> publicationList = null;

            publicationListView.Count = 0;

            var db = new Database("arisPublicWebDbDSN");
            string where = null;
            Sql sql = null;

            query = query.Replace(";", "").Replace("'", "''");

            if (type == "all")
            {
                where = "authors + '  ' +  title  + '  ' +  abstract like '%" + query + "%'";
            }
            else if (type == "author")
            {
                where = "authors like '%" + query + "%'";
            }
            else if (type == "title")
            {
                where = "title like '%" + query + "%'";
            }
            else if (type == "abstract")
            {
                where = "abstract like '%" + query + "%'";
            }

            sql = new Sql()
                 .Select("*")
                 .From("V_PUBLICATION_SEARCH")
                 .Where(where);

            publicationList = db.Query<SearchPublication>(sql).ToList();

            if (publicationList != null && publicationList.Count > 0)
            {
                publicationListView.Count = publicationList.Count;

                publicationList = publicationList.OrderByDescending(p => p.Key).ToList();

                if (count > 0 && skip > 0)
                {
                    publicationList = publicationList.Skip(skip).Take(count).ToList();
                }
                else if (count > 0)
                {
                    publicationList = publicationList.Take(count).ToList();
                }

                publicationListView.PublicationList = publicationList;
            }

            return publicationListView;
        }


        public static UsdaPublicationSearch SearchPublications(string modeCode, int count = -1, int skip = 0, int filterYear = 0, string show = "")
        {
            UsdaPublicationSearch publicationSearch = new UsdaPublicationSearch();
            publicationSearch.Count = 0;

            List<UsdaPublication> publicationList = null;

            List<string> modeCodeArray = Helpers.ModeCodes.ModeCodeArray(modeCode);

            if (filterYear == 0)
            {
                filterYear = DateTime.Now.Year;
            }

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

                where += " AND LEFT(DATEPART(yyyy,journal_accpt_date), 4) = '" + filterYear + "'";

                if (show == "pubtype")
                {
                    where += " AND PUB_TYPE_CODE = 'J'";
                }

                sql = new Sql()
                 .Select("*")
                 .From("gen_public_115s")
                 .Where(where);

                publicationList = db.Query<UsdaPublication>(sql).ToList();

                if (publicationList != null && publicationList.Count > 0)
                {
                    publicationSearch.Count = publicationList.Count;

                    publicationList = publicationList.OrderByDescending(p => p.JournalAcceptDate).ToList();

                    if (count > 0 && skip > 0)
                    {
                        publicationList = publicationList.Skip(skip).Take(count).ToList();
                    }
                    else if (count > 0)
                    {
                        publicationList = publicationList.Take(count).ToList();
                    }

                    publicationSearch.PublicationList = publicationList;
                }
            }



            List<ModeCodeNew> modeCodeList = Helpers.Aris.ModeCodesNew.GetOldModeCode(modeCode);

            foreach (ModeCodeNew modeCodeItem in modeCodeList)
            {
                Sql sql2 = null;
                List<UsdaPublication> publicationList2 = null;

                List<string> modeCodeArray2 = Helpers.ModeCodes.ModeCodeArray(modeCodeItem.ModecodeOld);

                if (modeCodeArray2 != null && modeCodeArray2.Count == 4)
                {
                    string where = "MODECODE_1 = '" + modeCodeArray2[0] + "' AND ";

                    if (modeCodeArray2[1] != "00")
                    {
                        where += "MODECODE_2 = '" + modeCodeArray2[1] + "' AND ";
                    }
                    if (modeCodeArray2[2] != "00")
                    {
                        where += "MODECODE_3 = '" + modeCodeArray2[2] + "' AND ";
                    }
                    if (modeCodeArray2[3] != "00")
                    {
                        where += "MODECODE_4 = '" + modeCodeArray2[3] + "' AND ";
                    }

                    if (where.EndsWith(" AND "))
                    {
                        where = where.Substring(0, where.LastIndexOf(" AND "));
                    }

                    where += " AND LEFT(DATEPART(yyyy,journal_accpt_date), 4) = '" + filterYear + "'";

                    if (show == "pubtype")
                    {
                        where += " AND PUB_TYPE_CODE = 'J'";
                    }

                    sql2 = new Sql()
                     .Select("*")
                     .From("gen_public_115s")
                     .Where(where);

                    publicationList2 = db.Query<UsdaPublication>(sql2).ToList();

                    if (publicationList == null)
                    {
                        publicationList = publicationList2;
                    }
                    else
                    {
                        publicationList.AddRange(publicationList2);
                    }
                }

            }

            if (publicationList != null && publicationList.Count > 0)
            {
                publicationSearch.Count = publicationList.Count;

                publicationList = publicationList.OrderByDescending(p => p.JournalAcceptDate).ToList();

                if (count > 0 && skip > 0)
                {
                    publicationList = publicationList.Skip(skip).Take(count).ToList();
                }
                else if (count > 0)
                {
                    publicationList = publicationList.Take(count).ToList();
                }

                publicationSearch.PublicationList = publicationList;
            }


            return publicationSearch;
        }


        public static List<PeoplePublication> PublicationsByPerson(int personId)
        {
            List<PeoplePublication> peoplePublicationsList = null;

            PeopleInfo peopleInfo = Helpers.Aris.People.GetPerson(personId);

            if (peopleInfo != null)
            {
                var db = new Database("arisPublicWebDbDSN");

                Sql sql = null;

                sql = new Sql()
                 .Select("*")
                 .From("V_PERSON_115S")
                 .Where("EMP_ID = '"+ peopleInfo.EmpId +"'");

                peoplePublicationsList = db.Query<PeoplePublication>(sql).ToList();
            }


            return peoplePublicationsList;
        }


        public static List<ProjectPublication> GetPublicationsByProjectYear(int accountNo, string year)
        {
            List<ProjectPublication> projectPublicationList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"select 	publication, seq_no_115,
			            (
				            select 	count(*) 
				            from 	gen_public_115s
				            where 	seq_no_115 = a421_publications.seq_no_115
			            )  ok  
	            from 	a421_publications
	            where 	ACCN_NO = @accountNo
	            and 	fy = @year ";

            projectPublicationList = db.Query<ProjectPublication>(sql, new { accountNo = accountNo, year = year }).ToList();

            return projectPublicationList;
        }


        public static List<UsdaPublication> GetPublicationsByProject(int accountNo)
        {
            List<UsdaPublication> projectPublicationList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT 	manuscript_title, citation, seq_no_115, journal_accpt_date, PUB_TYPE_CODE,
					        reprint_url				
			        FROM 	gen_public_115s
			        WHERE 	ACCN_NO = @accountNo
			        AND 	journal_accpt_date IS NOT NULL
			        ORDER BY journal_accpt_date desc
			        Option  (Robust Plan)";

            projectPublicationList = db.Query<UsdaPublication>(sql, new { accountNo = accountNo }).ToList();

            return projectPublicationList;
        }


        public static string PublicationType(string pubCode)
        {
            if (pubCode == "A")
            {
                return "Abstract Only";
            }
            else if (pubCode == "J")
            {
                return "Peer Reviewed Journal";
            }
            else if (pubCode == "P")
            {
                return "Proceedings";
            }
            else
            {
                return "";
            }
        }

    }
}
