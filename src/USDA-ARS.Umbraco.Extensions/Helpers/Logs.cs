using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class Logs
   {
      public static void AddErrorUrlToLog(Uri url)
      {
         if (url != null && url.AbsoluteUri != null)
         {
            LogHelper.Error(typeof(Logs), "ERROR URL: " + url.AbsoluteUri, null);
         }
      }
   }
}
