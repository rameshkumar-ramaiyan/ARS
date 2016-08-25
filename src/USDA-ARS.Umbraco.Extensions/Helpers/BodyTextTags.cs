using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
   public class BodyTextTags
   {
      public static string ReplaceTags(string text, string modeCode)
      {
         if (false == string.IsNullOrEmpty(text))
         {
            if (text.IndexOf("{{CONTACT_US_LINKS}}") >= 0)
            {
               text = ReplaceTagContactUs(text, modeCode);
            }

            if (true == text.Contains("[MODE_CODE:NAME]"))
            {
               IPublishedContent node = Nodes.GetNodeByModeCode(modeCode);

               if (node != null)
               {
                  text = text.Replace("[MODE_CODE:NAME]", node.Name);
               }
            }
         }

         return text;
      }


      public static string Modify300DpiDownload(IPublishedContent node, string text)
      {
         if (text.Contains("href=\"/isi/index.htm\""))
         {
            // Find 300 dpi link
            Match m2 = Regex.Match(text, "(<A HREF=\"(http://www.ars.usda.gov)*/isi/index.htm\">)(.*?)(</A>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (m2.Success)
            {
               // /oc/images/graphics/photos/feb07/d593-1/
               // /is/graphics/photos/300dpi/kesa/d593-1.jpg
               string href = "<a href=\"/ARSUserFiles/oc/graphics/photos/300dpi/kesa/" + node.Name.ToLower() + ".jpg\" target=\"_blank\">" + m2.Groups[3] + "</a>";

               text = text.Replace(m2.Groups[0].Value, href);
            }

            // Find thumbnail link
            Match m3 = Regex.Match(text, "(<A HREF=\"([^\"]*)\">)(.*?)(</A>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (m3.Success)
            {
               string hrefLink = m3.Groups[2].Value;
               string imagePath = "";

               if (hrefLink.IndexOf("/") < 0)
               {
                  Uri nodeUri = new Uri("http://www.ars.usda.gov" + node.Url);
                  int i = 0;
                  foreach (string segment in nodeUri.Segments)
                  {
                     if (i < nodeUri.Segments.Count() - 1)
                     {
                        imagePath += segment;
                     }

                     i++;
                  }

                  imagePath += hrefLink;

                  // /ARSUserFiles/oc/graphics/photos/jan05/d004-1.jpg

                  imagePath = imagePath.ToLower().Replace("/oc/images/", "/ARSUserFiles/oc/graphics/");
               }

               string href = "<a href=\"" + imagePath + "\" target=\"_blank\">" + m3.Groups[3] + "</a>";

               text = text.Replace(m3.Groups[0].Value, href);
            }
         }


         return text;
      }


      private static string ReplaceTagContactUs(string text, string modeCode)
      {
         string output = "";
         IPublishedContent homeNode = Nodes.Homepage();

         output += "<table width=\"368\">\r\n";
         output += " <tbody><tr>\r\n";
         output += " <td class=\"HdrBlackBold\">General Feedback</td>\r\n";
         output += " </tr>\r\n";
         output += " <tr>\r\n";
         output += " <td><img src=\"/images/content-divider.gif\" alt=\"divider line\" name=\"Divider line\" width=\"368\" height=\"10\"></td>\r\n";
         output += " </tr>\r\n";

         if (false == string.IsNullOrEmpty(modeCode))
         {
            IPublishedContent siteNode = Nodes.GetNodeByModeCode(modeCode, false);

            if (siteNode != null)
            {
               output += "<tr><td class=\"BodyTextBlack\">";
               output += "If you would like to submit comments, questions or provide general feedback ";
               output += "on the <strong>" + siteNode.Name + "</strong> Web site, please select a ";
               output += "category below to have your comment emailed to the appropriate person.";
               output += "</td></tr>";

               output += GenerateContactLinks(siteNode);
            }
         }
         else if (homeNode != null)
         {
            output += "<tr><td class=\"BodyTextBlack\">";
            output += "If you would like to submit comments, questions or provide general feedback ";
            output += "on the <strong>ARS</strong> Web site, please select a ";
            output += "category below to have your comment emailed to the appropriate office.";
            output += "</td></tr>";

            output += GenerateContactLinks(homeNode);
         }

         output += " </tbody>\r\n";
         output += " </table>\r\n";

         output = text.Replace("{{CONTACT_US_LINKS}}", output);

         return output;
      }


      private static string GenerateContactLinks(IPublishedContent node)
      {
         string output = "";

         IPublishedContent settingsNode = Nodes.SiteSettings();
         string emailto = settingsNode.GetPropertyValue<string>("defaultContactEmail");
         string emailSubject = settingsNode.GetPropertyValue<string>("contactEmailSubject");
         string defaultContactLabel = settingsNode.GetPropertyValue<string>("defaultContactLabel");

         ArchetypeModel contactCategoryList = node.GetPropertyValue<ArchetypeModel>("contactCategory");

         if (contactCategoryList != null && contactCategoryList.Any())
         {
            output += "<tr><td>";
            output += "<ul>";

            foreach (var categoryItem in contactCategoryList)
            {
               string customEmailTo = "";

               if (categoryItem.HasValue("contactPerson"))
               {
                  PeopleInfo person = Aris.People.GetPerson(Convert.ToInt32(categoryItem.GetValue("contactPerson")));

                  if (person != null)
                  {
                     customEmailTo += person.Email + ";";
                  }
               }

               if (true == categoryItem.HasValue("customEmail"))
               {
                  customEmailTo += categoryItem.GetValue<string>("customEmail") + ";";
               }

               // Set default if email hasn't been filled out
               if (true == string.IsNullOrEmpty(customEmailTo))
               {
                  customEmailTo = emailto;
               }

               output += "<li><a href=\"mailto:" + customEmailTo + "?Subject=" + emailSubject + categoryItem.GetValue("category") + "\" id=\"anch_48\">";
               output += categoryItem.GetValue("category") + "</a></li>";
            }

            output += "</ul>";
            output += "</td></tr>";
         }
         else
         {
            string customEmailTo = "";

            customEmailTo = emailto;

            customEmailTo = customEmailTo.TrimEnd(';');

            output += "<tr><td>";
            output += "<ul>";
            output += "<li><a href=\"mailto:" + customEmailTo + "?Subject=" + emailSubject + defaultContactLabel + "\" id=\"anch_48\">" + defaultContactLabel + "</a></li>";
            output += "</ul>";
            output += "</td></tr>";
         }

         return output;
      }



   }
}
