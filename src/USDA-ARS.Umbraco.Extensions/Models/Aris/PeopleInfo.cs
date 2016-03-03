using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("w_people_info")]
    public class PeopleInfo
    {
        [Column("EMP_ID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string EmpId { get; set; }

        [Column("PERSONID")]
        public string PersonId { get; set; }

        [Column("modecode_1")]
        public int ModeCode1 { get; set; }
        [Column("modecode_2")]
        public int ModeCode2 { get; set; }
        [Column("modecode_3")]
        public int ModeCode3 { get; set; }
        [Column("modecodeE_4")]
        public int ModeCode4 { get; set; }

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

        [Column("officialtitle")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string TitleOfficial { get; set; }

        [Column("workingtitle")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string TitleWorking { get; set; }

        [Column("email")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [Column("ofcfax")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Fax { get; set; }

        [Column("ofcfaxareacode")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FaxAreaCode { get; set; }
        
        [Column("DESKPHONE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Phone { get; set; }

        [Column("deskareacode")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PhoneAreaCode { get; set; }

        [Column("deskext")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PhoneExt { get; set; }

        [Column("deskroomnum")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string RoomNumber { get; set; }

        [Column("DESKADDR1")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Address1 { get; set; }

        [Column("deskaddr2")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Address2 { get; set; }

        [Column("deskcity")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string City { get; set; }

        [Column("deskstate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string State { get; set; }

        [Column("homepageurl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string HomepageUrl { get; set; }

        [Column("deskzip4")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PostalCode { get; set; }

    }

}
