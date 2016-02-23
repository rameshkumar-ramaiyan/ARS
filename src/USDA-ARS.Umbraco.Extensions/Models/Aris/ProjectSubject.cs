using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectSubject
    {
        [Column("soi_code")]
        public int SoiCode { get; set; }

        [Column("prime_code")]
        public int PrimeCode { get; set; }

        [Column("long_desc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string LongDescription { get; set; }
    }
}

