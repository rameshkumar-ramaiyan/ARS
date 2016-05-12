using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportContactUs.Models.Aris
{
    public class SiteUserRole
    {
        [Column("siteRole")]
        public string SiteRole { get; set; }

        [Column("PersonID")]
        public int PersonID { get; set; }

        [Column("EMail")]
        public string Email { get; set; }

        [Column("site_code")]
        public string SiteCode { get; set; }
    }
}
