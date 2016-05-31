using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
    public class Navigation
    {
        [Column("navID")]
        public int NavId { get; set; }

        [Column("NavSys_ID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int NavSysId { get; set; }

        [Column("NavLabel")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string NavLabel { get; set; }

        [Column("NavURL")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string NavURL { get; set; }

        [Column("Parent_NavID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int ParentNavId { get; set; }

        [Column("rownum")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int RowNum { get; set; }

        [Column("NavTarget")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string NavTarget { get; set; }

    }
}
