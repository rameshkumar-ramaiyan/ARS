using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectAccomplishment
    {
        [Column("accomp_no")]
        public int AccomplishNo { get; set; }

        [Column("accomplishment")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Accomplishment { get; set; }
    }
}

