using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace USDA_ARS.ImportNews.Models
{
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
