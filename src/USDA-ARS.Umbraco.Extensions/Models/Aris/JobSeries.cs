using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("V_JOB_SERIES")]
    public class JobSeries
    {
        [Column("JOB_SERIES_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string JobSeriesCode { get; set; }

        [Column("SERIES_TITLE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SeriesTitle { get; set; }
    }
}
