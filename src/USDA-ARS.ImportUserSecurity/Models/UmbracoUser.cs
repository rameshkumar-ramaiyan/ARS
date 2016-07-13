using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportUserSecurity.Models
{
    public class UmbracoUser
    {
        public int id { get; set; }
        public int startStructureID { get; set; }
        public string userName { get; set; }
    }
}
