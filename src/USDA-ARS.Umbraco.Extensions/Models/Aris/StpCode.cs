using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;


namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class StpCode
    {
        [Column("STP_OBJ_CODE")]
        public int ObjCode { get; set; }

        [Column("STP_APP_CODE")]
        public int AppCode { get; set; }

        [Column("STP_ELE_CODE")]
        public int EleCode { get; set; }

        [Column("STP_PROB_CODE")]
        public int ProbCode { get; set; }

        [Column("LONG_DESC")]
        public string LongDesc { get; set; }

        [Column("SHORT_DESC")]
        public string ShortDesc { get; set; }

        [Column("STATUS_CODE")]
        public string StatusCode { get; set; }

        [Column("stp_code")]
        public string Code { get; set; }

        
    }
}
