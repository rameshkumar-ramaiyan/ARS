using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportNavigation.Models
{
   public class SubsiteLookup
   {
      public string Subsite { get; set; }
      public int UmbracoId { get; set; }
      public string Url { get; set; }
      public string OldUrl { get; set; }
   }
}
