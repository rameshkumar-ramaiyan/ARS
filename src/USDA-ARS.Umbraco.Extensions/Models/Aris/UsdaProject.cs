using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("V_CLEAN_PROJECTS")]
    public class UsdaProject
    {
        [Column("ACCN_NO")]
        public int AccountNo { get; set; }

        [Column("modecode_1")]
        public int ModeCode1 { get; set; }
        [Column("modecode_2")]
        public int ModeCode2 { get; set; }
        [Column("modecode_3")]
        public int ModeCode3 { get; set; }
        [Column("modecode_4")]
        public int ModeCode4 { get; set; }

        [Column("PRJ_TITLE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("ProjectNumber")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectNumber { get; set; }

        [Column("OBJECTIVE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Objective { get; set; }

        [Column("APPROACH")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Approach { get; set; }

        [Column("status_code")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string StatusCode { get; set; }

        [Column("prj_type")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectType { get; set; }
    }
}
