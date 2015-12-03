using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models
{
    [TableName("usdaArsNews")]
    public class UsdaArsNews
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int NewsID { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string SubjectField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ISFileName { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ToField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string FromField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string BodyField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string MessageNumberField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ReplytoField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Headerfield { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string AttachementsField { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        public string BodyOpener { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        public string bodyClosing { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        public string published { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime SPSysUpdateDate { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime SPSysBeginTime { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        public string SPSysUpdateUser_ID { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime SPSysEndTime { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string OriginSite_Type { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string OriginSite_ID { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int MessageID { get; set; }
    }
}
