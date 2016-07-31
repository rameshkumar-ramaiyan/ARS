using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportDocs.Models
{
   [TableName("cmsPropertyData")]
   public class UmbracoPropertyData
   {
      [Column("dataNvarchar")]
      public string DataNVarchar { get; set; }

      [Column("dataNtext")]
      public string DataNtext { get; set; }

      [Column("text")]
      public string Title { get; set; }

      [Column("contentNodeId")]
      public int UmbracoId { get; set; }

      [Column("uniqueID")]
      public Guid UmbracoGuid { get; set; }
   }
}
