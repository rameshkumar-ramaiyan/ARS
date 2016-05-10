using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZetaHtmlCompressor;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class CleanHtml
    {
        public static string CleanUpHtml(string bodyText)
        {
            string output = "";

            HtmlCompressor htmlCompressor = new HtmlCompressor();
            htmlCompressor.setRemoveMultiSpaces(true);
            htmlCompressor.setRemoveIntertagSpaces(true);

            output = htmlCompressor.compress(bodyText);

            return output;
        }
    }
}