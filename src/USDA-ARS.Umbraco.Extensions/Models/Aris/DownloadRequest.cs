using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    [TableName("DownloadRequests")]
    [PrimaryKey("downreqid", autoIncrement = true)]
    public class DownloadRequest
    {
        [Column("downreqid")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SoftwareId")]
        public Guid SoftwareId { get; set; }

        [Column("perfname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FirstName { get; set; }

        [Column("perlname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string LastName { get; set; }

        [Column("permname")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string MiddleName { get; set; }

        [Column("Email")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [Column("Affiliation")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Affiliation { get; set; }

        [Column("Purpose")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Purpose { get; set; }

        [Column("Comments")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Comments { get; set; }

        [Column("TimeStamp")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime TimeStamp { get; set; }

        [Column("HTTP_REFERER")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string HttpReferer { get; set; }

        [Column("REMOTE_ADDR")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string RemoteAddr { get; set; }

        [Column("City")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string City { get; set; }

        [Column("State")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string State { get; set; }

        [Column("Country")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Country { get; set; }

        [Column("Reference")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Reference { get; set; }

        [Column("Position")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Position { get; set; }

        [Column("SpSysEndTime")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime SpSysEndTime { get; set; }
    }
}
