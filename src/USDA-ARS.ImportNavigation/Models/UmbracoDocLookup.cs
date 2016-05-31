using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.ImportNavigation.Models
{
    public class UmbracoDocLookup
    {
        [Column("dataNvarchar")]
        public string DocId { get; set; }

        [Column("dataNtext")]
        public string OldUrl { get; set; }

        [Column("contentNodeId")]
        public int UmbracoId { get; set; }
    }
}
