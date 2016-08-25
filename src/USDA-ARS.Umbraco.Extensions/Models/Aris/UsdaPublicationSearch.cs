using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
   public class UsdaPublicationSearch
   {
      public List<UsdaPublication> PublicationList { get; set; }
      public int Count { get; set; }
      public List<int> YearsList { get; set; }

   }
}
