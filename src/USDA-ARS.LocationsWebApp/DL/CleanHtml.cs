using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using ZetaHtmlCompressor;

namespace USDA_ARS.LocationsWebApp.DL
{
    public class CleanHtml
    {
        public static string CleanUpHtml(string bodyText, string modeCode = "")
        {
            string output = "";

            if (false == string.IsNullOrWhiteSpace(bodyText))
            {
                HtmlCompressor htmlCompressor = new HtmlCompressor();
                htmlCompressor.setRemoveMultiSpaces(true);
                htmlCompressor.setRemoveIntertagSpaces(true);

                bodyText = bodyText.Replace("—", "-");

                bodyText = ReplaceUnicodeText(bodyText);

                bodyText = Regex.Replace(bodyText, @"/pandp/people/people\.htm\?personid\=", "/people-locations/person?person-id=", RegexOptions.IgnoreCase);

                bodyText = Regex.Replace(bodyText, @"http(s)*://www\.ars\.usda\.gov", "", RegexOptions.IgnoreCase);
                bodyText = Regex.Replace(bodyText, @"http(s)*://ars\.usda\.gov", "", RegexOptions.IgnoreCase);

                bodyText = Regex.Replace(bodyText, @"http://", "https://");

                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/place", "/ARSUserFiles", RegexOptions.IgnoreCase);
                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/people", "/ARSUserFiles", RegexOptions.IgnoreCase);
                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/person", "/ARSUserFiles", RegexOptions.IgnoreCase);
                bodyText = Regex.Replace(bodyText, @"/sp2userfiles/Program/", "/ARSUserFiles/np", RegexOptions.IgnoreCase);

                bodyText = Regex.Replace(bodyText, @"\""/images/", "\"/ARSUserFiles/images/", RegexOptions.IgnoreCase);
                bodyText = Regex.Replace(bodyText, @"\""/incme/", "\"/ARSUserFiles/incme/", RegexOptions.IgnoreCase);

                try
                {
                    Regex ItemRegex = new Regex(@"/[\d]{8}/", RegexOptions.IgnoreCase);
                    foreach (Match ItemMatch in ItemRegex.Matches(bodyText))
                    {
                        System.Data.DataTable legacyOldModeCodes = new System.Data.DataTable();

                        if (ItemMatch != null && ItemMatch != null)
                        {
                            legacyOldModeCodes = AddRetrieveLocationsDL.GetNewModeCodesBasedOnOldModeCodes(ItemMatch.Value.Replace("/", ""));

                            if (legacyOldModeCodes.Rows.Count > 0)
                            {
                                bodyText = Regex.Replace(bodyText, ItemMatch.Value, "/" + legacyOldModeCodes.Rows[0].Field<string>(1) + "/", RegexOptions.IgnoreCase);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                output = htmlCompressor.compress(bodyText);
            }

            return output;
        }


        public static string ReplaceUnicodeText(string text)
        {
            if (false == string.IsNullOrEmpty(text))
            {
                 var bytes = Encoding.Default.GetBytes(text);
                 text = Encoding.UTF8.GetString(bytes);
              
                text = Regex.Replace(text, "[\u2018\u2019\u201A]", "'");
                // smart double quotes
                text = Regex.Replace(text, "[\u201C\u201D\u201E]", "\"");
                // ellipsis
                text = Regex.Replace(text, "\u2026", "...");
                // dashes
                text = Regex.Replace(text, "[\u2013\u2014]", "-");
                text = Regex.Replace(text, "Â·", ".");
                text = Regex.Replace(text, "â€¢", ".");
            }

            return text;
        }
    }
}