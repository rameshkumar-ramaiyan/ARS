using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (text.IndexOf("{{CONTACT_US_LINKS}}") >= 0)
            {
                text = ReplaceTagContactUs(text, modeCode);
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
                IPublishedContent siteNode = Nodes.GetNodeByModeCode(modeCode);

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

            if (homeNode != null)
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
            string emailto = settingsNode.GetPropertyValue<string>("defaultContact");

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

                    output += "<li><a href=\"mailto:" + customEmailTo + "?Subject=From ARS website: " + categoryItem.GetValue("category") + "\" target=\"_blank\" id=\"anch_48\">";
                    output += categoryItem.GetValue("category") + "</a></li>";
                }

                output += "</ul>";
                output += "</td></tr>";
            }
            else
            {
                string customEmailTo = "";

                if (node.HasValue("contactPerson"))
                {
                    PeopleInfo person = Aris.People.GetPerson(Convert.ToInt32(node.GetPropertyValue("contactPerson")));

                    if (person != null)
                    {
                        customEmailTo += person.Email + ";";
                    }
                }

                if (false == string.IsNullOrEmpty(node.GetPropertyValue<string>("customContactEmail")))
                {
                    customEmailTo += node.GetPropertyValue<string>("customContactEmail") + ";";
                }

                // Set default if email hasn't been filled out
                if (true == string.IsNullOrEmpty(customEmailTo))
                {
                    customEmailTo = emailto;
                }

                customEmailTo = customEmailTo.TrimEnd(';');

                output += "<tr><td>";
                output += "<ul>";
                output += "<li><a href=\"mailto:" + customEmailTo + "?Subject=From ARS website: ARS Webmaster\" target=\"_blank\" id=\"anch_48\">ARS Webmaster</a></li>";
                output += "</ul>";
                output += "</td></tr>";
            }

            return output;
        }
    }
}
