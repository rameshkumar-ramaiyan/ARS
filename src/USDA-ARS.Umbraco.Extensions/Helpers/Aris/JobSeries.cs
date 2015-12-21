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
    public class JobSeries
    {

        public static List<Models.Aris.JobSeries> GetJobSeriesList()
        {
            List<Models.Aris.JobSeries> jobSeriesList = null;

            var db = new Database("arisPublicWebDbDSN");

            Sql sql = null;

            string where = "JOB_SERIES_CODE <> '0000'";

            sql = new Sql()
             .Select("*")
             .From("V_JOB_SERIES")
             .Where(where)
             .OrderBy("SERIES_TITLE");

            jobSeriesList = db.Query<Models.Aris.JobSeries>(sql).ToList();

            return jobSeriesList;
        }

}
}
