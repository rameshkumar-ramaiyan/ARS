using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportLocations.Models
{
    public class City
    {
        public int UmbracoId { get; set; }
        public string Name { get; set; }
        public string ModeCode { get; set; }
        public string StateCode { get; set; }
    }
}
