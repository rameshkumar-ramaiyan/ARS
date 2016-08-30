using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
   public class CityState
   {
      [Column("city")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string City { get; set; }

      [Column("state_code")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string StateCode { get; set; }

      [Column("location")]
      [NullSetting(NullSetting = NullSettings.Null)]
      public string Location { get; set; }

   }
}
