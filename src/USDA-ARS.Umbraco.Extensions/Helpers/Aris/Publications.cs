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

                where += " AND LEFT(DATEPART(yyyy,journal_accpt_date), 4) = '"+ DateTime.Now.Year +"'";

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
    }
}
