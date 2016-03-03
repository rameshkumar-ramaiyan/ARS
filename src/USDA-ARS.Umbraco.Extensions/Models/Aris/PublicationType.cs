using System.ComponentModel.DataAnnotations.Schema;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class PublicationType
    {
        [Column("pub_type_code")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PublicationTypeCode { get; set; }

        [Column("description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Description { get; set; }
    }
}
