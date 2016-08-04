using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class Scripts
   {
      public string PageHeaderScriptFiles { get; set; }
      public string PageFooterScriptFiles { get; set; }


      public void GenerateScriptFiles(IPublishedContent node)
      {
         if (node != null)
         {
            if (node.HasValue("pageHeaderScriptFiles"))
            {
               PageHeaderScriptFiles = "\r\n<!-- Custom Page Header Scripts -->\r\n";

               ArchetypeModel pageHeaderScriptFiles = node.GetPropertyValue<ArchetypeModel>("pageHeaderScriptFiles");

               if (pageHeaderScriptFiles != null && pageHeaderScriptFiles.Any())
               {
                  foreach (var scriptFile in pageHeaderScriptFiles)
                  {
                     string file = scriptFile.GetValue<string>("filePath");

                     PageHeaderScriptFiles += GenerateHtmlImportLine(file);
                  }
               }
            }


            if (node.HasValue("pageFooterScriptFiles"))
            {
               PageFooterScriptFiles = "\r\n<!-- Custom Page Footers Scripts -->\r\n";

               ArchetypeModel pageFooterScriptFiles = node.GetPropertyValue<ArchetypeModel>("pageFooterScriptFiles");

               if (pageFooterScriptFiles != null && pageFooterScriptFiles.Any())
               {
                  foreach (var scriptFile in pageFooterScriptFiles)
                  {
                     string file = scriptFile.GetValue<string>("filePath");

                     PageFooterScriptFiles += GenerateHtmlImportLine(file);
                  }
               }
            }
         }
      }


      private string GenerateHtmlImportLine(string file)
      {
         string output = null;

         if (false == string.IsNullOrEmpty(file))
         {
            if (file.ToLower().EndsWith(".js"))
            {
               output = "<script src=\"" + file + "\"></script>\r\n";
            }
            else if (file.ToLower().EndsWith(".css"))
            {
               output = "<link href=\""+ file + "\" rel=\"stylesheet\" type=\"text/css\" />\r\n";
            }
            else
            {
               output = "<!-- Unable to insert file: "+ file +" -->\r\n";
            }
         }

         return output;
      }
   }
}
