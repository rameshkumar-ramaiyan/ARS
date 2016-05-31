using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportNavigation.Models
{
    public class ImportedNav
    {
        public int NavSysId { get; set; }
        public Guid UmbracoGuid { get; set; }
        public string NavTitle { get; set; }
        public string Section { get; set; }
        public string Label { get; set; }
    }
}
