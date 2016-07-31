using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    [TableName("cmsPropertyData")]
    public class UmbracoPropertyData
    {
        [Column("dataInt")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DataInt { get; set; }

        [Column("dataNvarchar")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DataNvarchar { get; set; }

        [Column("dataNtext")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DataNtext { get; set; }

        [Column("contentNodeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int UmbracoId { get; set; }
    }
}
