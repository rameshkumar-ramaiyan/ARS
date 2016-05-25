using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
    [TableName("Sites")]
    public class Site
    {
        [Column("siteID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int siteId { get; set; }

        [Column("siteLabel")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteLabel { get; set; }

        [Column("site_Type")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteType { get; set; }

        [Column("site_Code")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteCode { get; set; }
    }
}
