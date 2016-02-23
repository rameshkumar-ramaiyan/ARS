using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectTeam
    {
        [Column("PERSONID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int PersonId { get; set; }

        [Column("PRIME_INDICATOR")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PrimeIndicator { get; set; }

        [Column("PERLNAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string LastName { get; set; }

        [Column("PERFNAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FirstName { get; set; }

        [Column("PERCOMMONNAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CommonName { get; set; }
    }
}
