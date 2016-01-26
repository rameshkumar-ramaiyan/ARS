using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("V_PEOPLE_INFO_2_DIRECTORY")]
    public class PeopleByCity
    {
        [Column("mySiteCode")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteCode { get; set; }

        [Column("modecodeconc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCodeConcat { get; set; }

        [Column("personid")]
        public int PersonId { get; set; }

        [Column("perfname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FirstName { get; set; }

        [Column("perlname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string LastName { get; set; }

        [Column("permname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string MiddleName { get; set; }

        [Column("percommonname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CommonName { get; set; }

        [Column("workingtitle")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string TitleWorking { get; set; }

        [Column("Email")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [Column("DeskPhone")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Phone { get; set; }

        [Column("deskareacode")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PhoneAreaCode { get; set; }

        [Column("city")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string City { get; set; }

        [Column("state_code")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string StateCode { get; set; }

        [Column("siteLabel")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string SiteLabel { get; set; }

        [Column("URLModecode")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCode { get; set; }
    }
}
