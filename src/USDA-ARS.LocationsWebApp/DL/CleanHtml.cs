using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using USDA_ARS.Umbraco.Extensions.Models.Aris;
using ZetaHtmlCompressor;

namespace USDA_ARS.LocationsWebApp.DL
{
   public class CleanHtml
   {
      public static string CleanUpHtml(string bodyText, string modeCode = "", List<ModeCodeNew> modeCodeNewList = null)
      {
         string output = "";

         if (false == string.IsNullOrWhiteSpace(bodyText))
         {
            HtmlCompressor htmlCompressor = new HtmlCompressor();
            htmlCompressor.setRemoveMultiSpaces(true);
            htmlCompressor.setRemoveIntertagSpaces(true);

            bodyText = bodyText.Replace("—", "-");
            bodyText = bodyText.Replace("�", "&bull;");
            bodyText = bodyText.Replace("·", "&bull;");

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
            bodyText = Regex.Replace(bodyText, @"src\=\""/is/", "src=\"/ARSUserFiles/oc/", RegexOptions.IgnoreCase);


            bodyText = Regex.Replace(bodyText, @"/News/Docs\.htm\?docid\=23712", "/{localLink:8002}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/News/Docs\.htm\?docid\=23559", "/{localLink:1145}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/is/graphics/photos/", "/{localLink:1145}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/is/pr/index\.html", "/{localLink:6996}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/News/docs\.htm\?docid\=6697", "/{localLink:9134}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/News/docs\.htm\?docid\=1383", "/{localLink:8030}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/News/docs\.htm\?docid\=1281", "/{localLink:8003}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/news/events\.htm", "/{localLink:8024}", RegexOptions.IgnoreCase);
            bodyText = Regex.Replace(bodyText, @"/news/events\.htm", "/{localLink:8024}", RegexOptions.IgnoreCase);

            try
            {
               if (modeCodeNewList != null && modeCodeNewList.Any())
               {
                  MatchCollection matches = Regex.Matches(bodyText, @"/SP2UserFiles/ad_hoc/([\d]{8})([^/]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                  if (matches.Count > 0)
                  {
                     foreach (Match match in matches)
                     {
                        if (match.Groups.Count == 3)
                        {
                           string modeCodeFound = match.Groups[1].Value;
                           string adHocFolder = match.Groups[2].Value;

                           string modeCodeNew = GetNewModeCode(modeCodeFound, modeCodeNewList);

                           bodyText = bodyText.Replace(match.Groups[0].Value, "/ARSUserFiles/" + modeCodeNew + "/" + adHocFolder);
                        }
                     }
                  }
               }
            }
            catch (Exception ex)
            {

            }

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
            text = Regex.Replace(text, "Â·", "&bull;");
            text = Regex.Replace(text, "â€¢", "&bull;");
            text = text.Replace("—", "-");
            text = text.Replace("�", "&bull;");
            text = text.Replace("·", "&bull;");
         }

         return text;
      }


      public static string GetNewModeCode(string oldModeCode, List<ModeCodeNew> modeCodeNewList)
      {
         string newModeCode = "";

         if (modeCodeNewList != null && modeCodeNewList.Count > 0)
         {
            ModeCodeNew modeCodeItemNew = modeCodeNewList.Where(p => p.ModecodeNew == oldModeCode).FirstOrDefault();

            if (modeCodeItemNew != null)
            {
               newModeCode = modeCodeItemNew.ModecodeNew;
            }
            else
            {
               ModeCodeNew modeCodeItemOld = modeCodeNewList.Where(p => p.ModecodeOld == oldModeCode).FirstOrDefault();

               if (modeCodeItemOld != null)
               {
                  newModeCode = modeCodeItemNew.ModecodeNew;
               }
            }
         }

         return newModeCode;
      }
   }
}