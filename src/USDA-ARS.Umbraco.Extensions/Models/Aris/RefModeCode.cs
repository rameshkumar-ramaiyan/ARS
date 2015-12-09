using Umbraco.Core.Persistence;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("REF_MODECODE")]
    public class RefModeCode
    {
        [Column("MODECODE_1")]
        public int ModeCode1 { get; set; }
        [Column("MODECODE_2")]
        public int ModeCode2 { get; set; }
        [Column("MODECODE_3")]
        public int ModeCode3 { get; set; }
        [Column("MODECODE_4")]
        public int ModeCode4 { get; set; }

        [Column("RL_EMP_ID")]
        public string RlEmpId { get; set; }

        [Column("RL_EMAIL")]
        public string RsEmail { get; set; }

        [Column("RL_FAX")]
        public string RsFax { get; set; }

        [Column("RL_TITLE")]
        public string RsTitle { get; set; }

        [Column("RL_PHONE")]
        public string RsPhone { get; set; }

        [Column("ADD_LINE_1")]
        public string RsAddress1 { get; set; }

        [Column("ADD_LINE_2")]
        public string RsAddress2 { get; set; }

        [Column("CITY")]
        public string RsCity { get; set; }

        [Column("STATE_CODE")]
        public string RsStateCode { get; set; }

        [Column("POSTAL_CODE")]
        public string RsPostalCode { get; set; }

        [Column("COUNTRY_CODE")]
        public string RsCountryCode { get; set; }

        [Column("MISSION_STATEMENT")]
        public string MissionStatement { get; set; }
    }
}
