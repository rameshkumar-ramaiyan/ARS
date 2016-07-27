using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
   [TableName("NavSystems")]
   public class NavSystem
   {
      [Column("NavSysID")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public int NavSysId { get; set; }

      [Column("NavSysLabel")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string NavSysLabel { get; set; }

      [Column("BBSect")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string BBSect { get; set; }

      [Column("OriginSite_Type")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string OriginSiteType { get; set; }

      [Column("OriginSite_ID")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string OriginSiteId { get; set; }

      [Column("Published")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string Published { get; set; }

      [Column("navPageLoc")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string NavPageLoc { get; set; }

      [Column("showLabel")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public bool ShowLabel { get; set; }


      public string ParentNodeTitle { get; set; }

   }
}
