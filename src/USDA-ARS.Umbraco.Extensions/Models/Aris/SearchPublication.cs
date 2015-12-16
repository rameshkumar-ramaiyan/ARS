using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("V_PUBLICATION_SEARCH")]
    public class SearchPublication
    {
        [Column("key")]
        public int Key { get; set; }

        [Column("title")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("abstract")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Abstract { get; set; }

        [Column("authors")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Authors { get; set; }
    }
}

