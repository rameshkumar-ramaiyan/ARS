using Archetype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using USDA_ARS.Umbraco.Extensions.Helpers.Aris;
using USDA_ARS.Umbraco.Extensions.Models;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class Software
    {
        public static UmbracoHelper UmbHelper = new UmbracoHelper(UmbracoContext.Current);


        public static ArchetypeFieldsetModel GetSoftwareById(string id)
        {
            ArchetypeFieldsetModel archetypeFielsetModel = null;
            List<IPublishedContent> softwareList = new List<IPublishedContent>();

            softwareList = GetSoftwareNodes();

            if (softwareList != null && softwareList.Any())
            {
                archetypeFielsetModel = softwareList.SelectMany(p => p.GetPropertyValue<ArchetypeModel>("software")).Where(t => t.GetValue<string>("softwareId") == id).FirstOrDefault();
            }


            return archetypeFielsetModel;
        }


        public static int GetLastSoftwareId()
        {
            int softwareId = 0;

            List<IPublishedContent> softwareList = GetSoftwareNodes();

            if (softwareList != null && softwareList.Any())
            {
                foreach (IPublishedContent softwareNode in softwareList)
                {
                    ArchetypeModel archetypeModel = softwareNode.GetPropertyValue<ArchetypeModel>("software");

                    foreach (ArchetypeFieldsetModel fieldsetModel in archetypeModel.Fieldsets)
                    {
                        int intTry = 0;
                        string softwareIdStr = fieldsetModel.GetValue<string>("softwareID");

                        if (int.TryParse(softwareIdStr, out intTry))
                        {
                            if (softwareId < intTry)
                            {
                                softwareId = intTry;
                            }
                        }
                    }
                }
            }


            return softwareId;
        }


        public static List<ArchetypeModel> GetAllSoftware()
        {
            List<ArchetypeModel> softwareList = new List<ArchetypeModel>();

            List<IPublishedContent> softwareNodeList = GetSoftwareNodes();

            if (softwareNodeList != null && softwareNodeList.Any())
            {
                foreach (IPublishedContent node in softwareList)
                {
                    if (node != null && node.HasValue("software") && 
                            node.GetPropertyValue<ArchetypeModel>("software").Fieldsets != null && 
                            node.GetPropertyValue<ArchetypeModel>("software").Fieldsets.Count() > 0)

                    softwareList.Add(node.GetPropertyValue<ArchetypeModel>("software"));
                }
            }
            
            return softwareList;
        }


        public static List<IPublishedContent> GetSoftwareNodes()
        {
            List<IPublishedContent> softwareList = new List<IPublishedContent>();
            List<IPublishedContent> nodeList = null;

            foreach (IPublishedContent root in UmbHelper.TypedContentAtRoot())
            {
                nodeList = root.Descendants().Where(n => n.HasValue("software") && false == string.IsNullOrEmpty(n.GetPropertyValue<string>("modeCode"))).ToList();

                if (nodeList != null && nodeList.Any())
                {
                    softwareList.AddRange(nodeList);
                }

                if (root.HasValue("software") && false == string.IsNullOrEmpty(root.GetPropertyValue<string>("modeCode")))
                {
                    softwareList.Add(root);
                }
            }

            if (softwareList != null && softwareList.Any())
            {
                softwareList = softwareList.OrderBy(p => p.GetPropertyValue<string>("modeCode")).ToList();
            }


            return softwareList;
        }


        public static string GetSoftwareLocation(IPublishedContent node)
        {
            string output = node.Name;

            if (node.DocumentTypeAlias == "ResearchUnit")
            {
                if (node.Level == 5)
                {
                    output += ", " + node.Parent.Parent.Name;
                }
                else if (node.Level == 4)
                {
                    output += ", " + node.Parent.Name;
                }
            }
            else if (node.DocumentTypeAlias == "Homepage")
            {
                output = "ARS, Washington, D.C.";
            }

            return output;
        }


        public static bool SendEmail(string emailTo, ArchetypeFieldsetModel softwareItem, Extensions.Models.Aris.DownloadRequest downloadRequest)
        {
            bool success = false;

            IPublishedContent emailTemplate = Nodes.GetEmailTemplate("Software Download");

            if (emailTemplate != null)
            {
                var message = new MailMessage();

                message.From = new MailAddress(emailTemplate.GetPropertyValue<string>("emailFrom"));

                foreach (string emailToItem in emailTo.Split(','))
                {
                    message.To.Add(new MailAddress(emailToItem.Trim()));
                }

                string emailCC = emailTemplate.GetPropertyValue<string>("emailCC");
                string emailBcc = emailTemplate.GetPropertyValue<string>("emailBcc");

                foreach (string emailCCItem in emailCC.Split(';'))
                {
                    if (false == string.IsNullOrWhiteSpace(emailCCItem))
                    {
                        message.CC.Add(new MailAddress(emailCCItem.Trim()));
                    }
                }

                foreach (string emailBccItem in emailBcc.Split(';'))
                {
                    if (false == string.IsNullOrWhiteSpace(emailBccItem))
                    {
                        message.Bcc.Add(new MailAddress(emailBccItem.Trim()));
                    }
                }

                message.Subject = emailTemplate.GetPropertyValue<string>("emailSubject").Trim();

                string body = emailTemplate.GetPropertyValue<string>("emailBody");


                body = body.Replace("src=\"/", "src=\"http://www.ars.usda.gov/");
                body = body.Replace("href=\"/", "href=\"http://www.ars.usda.gov/");


                // REPLACE CUSTOM SOFTWARE HTML TAGS
                if (softwareItem != null)
                {
                    body = body.Replace("[Software:SoftwareID]", softwareItem.GetValue<string>("softwareID"));
                    body = body.Replace("[Software:Title]", softwareItem.GetValue<string>("title"));
                }

                // REPLACE CUSTOM DOWNLOAD REQUEST HTML TAGS
                if (downloadRequest != null)
                {
                    foreach (var prop in downloadRequest.GetType().GetProperties())
                    {
                        string propValue = "";

                        if (prop.GetValue(downloadRequest, null) != null)
                        {
                            propValue = prop.GetValue(downloadRequest, null).ToString();
                        }

                        body = body.Replace("[Download:"+ prop.Name +"]", HttpContext.Current.Server.HtmlEncode(propValue));
                    }
                }

                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        smtp.Send(message);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<Software>("Problem sending software download email.", ex);
                    }
                }
            }

            return success;
        }
    }
}
