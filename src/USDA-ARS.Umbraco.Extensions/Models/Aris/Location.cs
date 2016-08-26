using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
   public class Location
   {
      [Column("modecodeconc")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string ModeCodeConcat { get; set; }

      [Column("modecode_1")]
      public string ModeCode1 { get; set; }
      [Column("modecode_2")]
      public string ModeCode2 { get; set; }
      [Column("modecode_3")]
      public string ModeCode3 { get; set; }
      [Column("modecode_4")]
      public string ModeCode4 { get; set; }

      [Column("STATUS_CODE")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string StatusCode { get; set; }

            
      [Column("MODECODE_1_DESC")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string ModeCode1Desc { get; set; }

      [Column("MODECODE_2_DESC")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string ModeCode2Desc { get; set; }

      [Column("MODECODE_3_DESC")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string ModeCode3Desc { get; set; }

      [Column("MODECODE_4_DESC")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string ModeCode4Desc { get; set; }


      [Column("RL_EMP_ID")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RlEmpId { get; set; }

      [Column("RL_EMAIL")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsEmail { get; set; }

      [Column("RL_FAX")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsFax { get; set; }

      [Column("RL_TITLE")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsTitle { get; set; }

      [Column("RL_PHONE")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsPhone { get; set; }

      [Column("ADD_LINE_1")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsAddress1 { get; set; }

      [Column("ADD_LINE_2")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsAddress2 { get; set; }

      [Column("CITY")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsCity { get; set; }

      [Column("STATE_CODE")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string RsStateCode { get; set; }

      [Column("POSTAL_CODE")]
      public string RsPostalCode { get; set; }

      [Column("COUNTRY_CODE")]
      public string RsCountryCode { get; set; }

      [Column("MISSION_STATEMENT")]
      public string MissionStatement { get; set; }

      [Column("web_label")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string WebLabel { get; set; }

      public int Level { get; set; }
   }
}
