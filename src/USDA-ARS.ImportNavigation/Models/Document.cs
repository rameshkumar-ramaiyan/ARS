using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
    [TableName("Documents")]
    public class Document
    {
        [Column("DocId")]
        public int DocId { get; set; }

        [Column("Title")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("DocType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DocType { get; set; }

        [Column("Published")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Published { get; set; }

        [Column("OriginSite_ID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string OriginSiteId { get; set; }

        [Column("OriginSite_Type")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteType { get; set; }

        [Column("RLNav")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int RLNav { get; set; }
    }
}
