using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("V_PERSON_115S")]
    public class PeoplePublication
    {
        [Column("JOURNAL_ACCPT_DATE")]
        public DateTime JournalAcceptDate { get; set; }

        [Column("EMP_ID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string EmpId { get; set; }

        [Column("SEQ_NO_115")]
        public int SeqNo115 { get; set; }

        [Column("AUTHORSHIP")]
        public int Authorship { get; set; }

        [Column("MANUSCRIPT_TITLE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("citation")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Citation { get; set; }

        [Column("PUB_TYPE_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PubTypeCode { get; set; }

        [Column("REPRINT_URL")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ReprintUrl { get; set; }


        
    }
}

