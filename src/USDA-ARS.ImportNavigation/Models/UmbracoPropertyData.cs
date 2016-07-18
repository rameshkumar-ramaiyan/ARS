using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
   [TableName("cmsPropertyData")]
   public class UmbracoPropertyData
   {
      public int id { get; set; }
      public int contentNodeId { get; set; }
      [NullSetting(NullSetting = NullSettings.Null)]
      public Guid versionId { get; set; }
      public int propertytypeid { get; set; }
      [NullSetting(NullSetting = NullSettings.Null)]
      public int dataInt { get; set; }
      [NullSetting(NullSetting = NullSettings.Null)]
      public DateTime? dataDate { get; set; }
      [NullSetting(NullSetting = NullSettings.Null)]
      public string dataNvarchar { get; set; }
      [NullSetting(NullSetting = NullSettings.Null)]
      public string dataNtext { get; set; }
   }
}
