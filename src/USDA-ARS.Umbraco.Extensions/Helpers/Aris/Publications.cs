using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
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



         List<ModeCodeNew> modeCodeList = Helpers.Aris.ModeCodesNews.GetOldModeCode(modeCode);

         if (modeCodeList != null)
         {
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

         string sql = @"select g.seq_no_115 as [key], MANUSCRIPT_TITLE as [title], abstract
				            from GEN_a115_authors_w_Names g, dbo.V_TEKTRAN_115S v ";

         if (type == "all")
         {
            sql += @"WHERE
                                (
                                    authors LIKE '%'+ @query +'%' OR
                                    manuscript_title LIKE '%'+ @query +'%' OR
                                    abstract LIKE '%'+ @query +'%'
								)
				            and g.seq_no_115 = v.seq_no_115";
         }
         else if (type == "title")
         {
            sql += @"where g.seq_no_115 = v.seq_no_115
                            and Manuscript_title like '%'+ @query +'%'";
         }
         else if (type == "author")
         {
            sql += @"where authors like '%'+ @query +'%'
                            and g.seq_no_115 = v.seq_no_115";
         }
         else if (type == "abstract")
         {
            sql += @"where g.seq_no_115 = v.seq_no_115
				            and abstract like '%'+ @query +'%'";
         }

         publicationList = db.Query<SearchPublication>(sql, new { query = query }).ToList();

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

         modeCode = ModeCodes.ModeCodeAddDashes(modeCode);

         List<string> modeCodeArray = ModeCodes.ModeCodeArray(modeCode);

         if (modeCodeArray != null && modeCodeArray.Count == 4)
         {
            string cacheKey = "SearchPublications:" + modeCode + ":" + show;
            int cacheUpdateInMinutes = 720;
            ObjectCache cache = MemoryCache.Default;

            List<UsdaPublication> publicationList = null;

            if (modeCode.EndsWith("00-00"))
            {
               publicationList = cache.Get(cacheKey) as List<UsdaPublication>;
            }

            if (publicationList == null)
            {
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

                  if (show == "pubtype")
                  {
                     where += " AND PUB_TYPE_CODE = 'J'";
                  }

                  sql = new Sql()
                   .Select("*")
                   .From("gen_public_115s")
                   .Where(where);

                  publicationList = db.Query<UsdaPublication>(sql).ToList();
               }

               List<ModeCodeNew> modeCodeList = Helpers.Aris.ModeCodesNews.GetOldModeCode(modeCode);

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

               if (modeCode.EndsWith("00-00"))
               {
                  CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                  cache.Add(cacheKey, publicationList, policy);
               }
            }

            if (publicationList != null && publicationList.Count > 0)
            {
               publicationSearch.YearsList = publicationList.DistinctBy(p => p.JournalAcceptDate.Year).Select(p => p.JournalAcceptDate.Year).ToList();

               if (publicationSearch.YearsList != null && publicationSearch.YearsList.Any())
               {
                  publicationSearch.YearsList = publicationSearch.YearsList.OrderByDescending(p => p).ToList();
               }

               if (filterYear == 0)
               {
                  filterYear = DateTime.Now.Year;
               }
               publicationList = publicationList.Where(p => p.JournalAcceptDate.Year == filterYear).ToList();

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


         return publicationSearch;
      }


      public static UsdaPublication GetPublicationById(int seqNo115)
      {
         UsdaPublication publication = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT 	*,
                        (	
	                        SELECT 	TOP 1 SUBSTRING(text_data, 1, 8000) 
	                        FROM	v_AH115_text t 
	                        WHERE 	t.seq_no_115 = a.seq_no_115 
	                        AND 	text_type = 'a'
                        ) AS Abstract2, J.NAME AS Journal
					
                        FROM 	gen_public_115s a, 
	                        REF_JOURNAL_OR_EQUIVALENT J
                        WHERE 	J.JOURNAL_OR_EQUIV_CODE = a.JOURNAL_CODE
	                        AND SEQ_NO_115 = @seqNo115";

         publication = db.Query<UsdaPublication>(sql, new { seqNo115 = seqNo115 }).FirstOrDefault();

         return publication;
      }



      public static List<PeoplePublication> PublicationsByPerson(int personId)
      {
         List<PeoplePublication> peoplePublicationsList = null;

         try
         {
            PeopleInfo peopleInfo = Helpers.Aris.People.GetPerson(personId);

            if (peopleInfo != null)
            {
               string empId = peopleInfo.EmpId;

               var db = new Database("arisPublicWebDbDSN");

               string sql = "SELECT * FROM V_PERSON_115S WHERE EMP_ID = @empId";

               peoplePublicationsList = db.Query<PeoplePublication>(sql, new { empId = empId }).ToList();

               if (peoplePublicationsList != null)
               {
                  peoplePublicationsList = peoplePublicationsList.OrderByDescending(p => p.JournalAcceptDate).ToList();
               }
            }
         }
         catch (Exception ex)
         {
            LogHelper.Error<Publications>("Could not load Publications by Person. ", ex);
         }


         return peoplePublicationsList;
      }


      public static List<ProjectPublication> GetPublicationsByProjectYear(int accountNo, string year)
      {
         List<ProjectPublication> projectPublicationList = null;

									int yearInt = 0;

									if (int.TryParse(year, out yearInt))
									{
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
									}

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


      public static List<UsdaPublication> GetPublicationsByStpCode(int stpCode)
      {
         List<UsdaPublication> projectPublicationList = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"select  p.seq_no_115, journal_pub_date, manuscript_title
		                    from	gen_public_115s p,
				                    v_115_stp_codes stp
		                    where 	p.seq_no_115 = stp.seq_no_115
		                    and 	stp_code = @stpCode
		                    and  	journal_pub_date is not null
		                    order by journal_pub_date desc";

         projectPublicationList = db.Query<UsdaPublication>(sql, new { stpCode = stpCode }).ToList();

         return projectPublicationList;
      }


      public static List<UsdaPublication> GetPublicationsRecent(DateTime dateStart, DateTime dateEnd)
      {
         List<UsdaPublication> projectPublicationList = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"select	* 
	                from	gen_public_115s
	                where	journal_pub_date between @dateStart and @dateEnd
	                order by journal_pub_date desc";

         projectPublicationList = db.Query<UsdaPublication>(sql, new { dateStart = dateStart, dateEnd = dateEnd }).ToList();

         return projectPublicationList;
      }


      public static List<StpCode> GetStpCodeList()
      {
         List<StpCode> stpCodeList = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT	cast(STP_OBJ_CODE as varchar(2)) + '' + 
				                    cast(STP_APP_CODE as varchar(2)) + '' + 
				                    cast(STP_ELE_CODE as varchar(2)) + '' + 
				                    cast(STP_PROB_CODE as varchar(2)) as stp_code,  
				                    SHORT_DESC
		                    FROM 	[ARIS_PUBLIC_WEB].[dbo].[REF_STP]
		                    where	cast(STP_OBJ_CODE as varchar(2)) + '' + 
				                    cast(STP_APP_CODE as varchar(2)) + '' + 
				                    cast(STP_ELE_CODE as varchar(2)) + '' + 
				                    cast(STP_PROB_CODE as varchar(2))
				                    in (	
                                        select 	distinct stp_code
		                                    from 	v_115_stp_codes	c, 
				                                    gen_public_115s p
		                                    where 	c.seq_no_115 = p.seq_no_115
		                                    and 	p.journal_pub_date is not null
					                    )
		                    order by short_desc";

         stpCodeList = db.Query<StpCode>(sql).ToList();

         return stpCodeList;
      }


      public static string PublicationType(string pubCode)
      {
         PublicationType pubType = null;

         var db = new Database("arisPublicWebDbDSN");

         string sql = @"SELECT	pub_type_code, description
			                FROM ref_115_pub
                            WHERE pub_type_code = @pubCode";

         pubType = db.Query<PublicationType>(sql, new { pubCode = pubCode }).FirstOrDefault();

         if (pubType != null)
         {
            return Utilities.Strings.UppercaseFirst(pubType.Description.ToLower());
         }
         else
         {
            return "Unknown";
         }
      }

   }
}
