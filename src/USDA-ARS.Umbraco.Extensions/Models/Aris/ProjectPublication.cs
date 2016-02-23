using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectPublication
    {
        [Column("publication")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Publication { get; set; }

        [Column("SEQ_NO_115")]
        public int SeqNo115 { get; set; }

        [Column("ok")]
        public int Ok { get; set; }
    }
}

