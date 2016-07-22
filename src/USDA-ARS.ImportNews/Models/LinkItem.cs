using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.ImportNews.Models
{
   public struct LinkItem
   {
      public string Href;
      public string Text;

      public override string ToString()
      {
         return Href + "\n\t" + Text;
      }
   }
}
