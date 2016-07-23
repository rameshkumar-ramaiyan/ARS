using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportNavigation.Models
{
   public class PersonLookup
   {
      public int PersonId { get; set; }
      public int UmbracoId { get; set; }
      public int NavUmbracoId { get; set; }
   }
}
