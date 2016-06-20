using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ZetaHtmlCompressor;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class CleanHtml
    {
        public static string CleanUpHtml(string bodyText)
        {
            string output = "";

            if (false == string.IsNullOrEmpty(bodyText))
            {
                HtmlCompressor htmlCompressor = new HtmlCompressor();
                htmlCompressor.setRemoveMultiSpaces(true);
                htmlCompressor.setRemoveIntertagSpaces(true);

                bodyText = Regex.Replace(bodyText, @"/pandp/people/people\.htm\?personid\=", "/people-locations/person?person-id=");

                bodyText = Regex.Replace(bodyText, @"http://www\.ars\.usda\.gov", "");
                bodyText = Regex.Replace(bodyText, @"http://ars\.usda\.gov", "");

                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/place", "/ARSUserFiles");
                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/people", "/ARSUserFiles");
                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/person", "/ARSUserFiles");

                output = htmlCompressor.compress(bodyText);
            }

            return output;
        }
    }
}