using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectProgram
    {
        [Column("NP_CODE")]
        public int NpCode { get; set; }

        [Column("short_desc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ShortDescription { get; set; }

        [Column("accn_no")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int AccountNo { get; set; }

        [Column("prj_title")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectTitle { get; set; }

        [Column("prj_type")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectType { get; set; }
    }
}

