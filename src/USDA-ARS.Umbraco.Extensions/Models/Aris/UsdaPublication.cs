using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("gen_public_115s")]
    public class UsdaPublication
    {
        [Column("ACCN_NO")]
        public int AccountNo { get; set; }

        [Column("SEQ_NO_115")]
        public int SeqNo115 { get; set; }

        [Column("modecode_1")]
        public int ModeCode1 { get; set; }
        [Column("modecode_2")]
        public int ModeCode2 { get; set; }
        [Column("modecode_3")]
        public int ModeCode3 { get; set; }
        [Column("modecode_4")]
        public int ModeCode4 { get; set; }

        [Column("ADD_LINE_1")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Address1 { get; set; }

        [Column("ADD_LINE_2")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Address2 { get; set; }

        [Column("MANUSCRIPT_TITLE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("PUB_TYPE_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PubTypeCode { get; set; }

        [Column("JOURNAL_ACCPT_DATE")]
        public DateTime JournalAcceptDate { get; set; }

        [Column("JOURNAL_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int JournalCode { get; set; }

        [Column("Journal")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Journal { get; set; }

        [Column("abstract")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Abstract { get; set; }

        [Column("summary")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Summary { get; set; }
    }
}
