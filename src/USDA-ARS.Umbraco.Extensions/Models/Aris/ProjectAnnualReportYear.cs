using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectAnnualReportYear
    {
        [Column("PY")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int Year { get; set; }
    }
}
