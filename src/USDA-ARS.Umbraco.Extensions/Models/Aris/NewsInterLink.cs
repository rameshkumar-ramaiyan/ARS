using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("NewsInterLinks")]
    public class NewsInterLink
    {
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UmbracoNodeId")]
        public int UmbracoNodeId { get; set; }

        [Column("UmbracoNodeGuid")]
        public Guid UmbracoNodeGuid { get; set; }

        [Column("LinkType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string LinkType { get; set; }

        [Column("LinkId")]
        public long LinkId { get; set; }
    }
}