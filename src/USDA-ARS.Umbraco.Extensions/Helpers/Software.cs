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


      public static IPublishedContent GetSoftwareById(string id)
      {
         IPublishedContent softwareItem = null;
         List<IPublishedContent> softwareList = new List<IPublishedContent>();

         softwareList = GetSoftwareNodes();

         if (softwareList != null && softwareList.Any())
         {
            softwareItem = softwareList.Where(p => p.GetPropertyValue<string>("softwareId") == id).FirstOrDefault();
         }

         return softwareItem;
      }


      public static int GetLastSoftwareId()
      {
         int softwareId = 0;

         List<IPublishedContent> softwareList = GetSoftwareNodes();

         if (softwareList != null && softwareList.Any())
         {
            foreach (IPublishedContent softwareNode in softwareList)
            {
               int intTry = 0;
               string softwareIdStr = softwareNode.GetPropertyValue<string>("softwareID");

               if (int.TryParse(softwareIdStr, out intTry))
               {
                  if (softwareId < intTry)
                  {
                     softwareId = intTry;
                  }
               }
            }
         }


         return softwareId;
      }


      public static List<IPublishedContent> GetAllSoftware()
      {
         List<IPublishedContent> softwareNodeList = GetSoftwareNodes();

         return softwareNodeList;
      }


      public static IEnumerable<IPublishedContent> GetSofwareListByNode(IPublishedContent node)
      {
         IEnumerable<IPublishedContent> softwareList = null;

         if (node != null && node.Children != null && node.Children.Any())
         {
            IPublishedContent softwareFolder = node.Children().Where(p => p.DocumentTypeAlias == "SiteSoftwareFolder").FirstOrDefault();
            
            if (softwareFolder != null)
            {
               softwareList = softwareFolder.Children;
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
            nodeList = root.Descendants().Where(n => false == string.IsNullOrEmpty(n.GetPropertyValue<string>("modeCode"))).ToList();

            if (nodeList != null && nodeList.Any())
            {
               foreach (IPublishedContent node in nodeList)
               {
                  IPublishedContent softwareFolder = node.Children.Where(p => p.DocumentTypeAlias == "SiteSoftwareFolder").FirstOrDefault();

                  if (softwareFolder != null)
                  {
                     softwareList.AddRange(softwareFolder.Children);
                  }
               }
            }

            if (false == string.IsNullOrEmpty(root.GetPropertyValue<string>("modeCode")))
            {
               IPublishedContent softwareFolder = root.Children.Where(p => p.DocumentTypeAlias == "SiteSoftwareFolder").FirstOrDefault();

               if (softwareFolder != null)
               {
                  softwareList.AddRange(softwareFolder.Children);
               }
            }
         }

         if (softwareList != null && softwareList.Any())
         {
            softwareList = softwareList.OrderBy(p => p.Parent.Parent.GetPropertyValue<string>("modeCode")).ThenBy(x => x.Name).ToList();
         }


         return softwareList;
      }


      public static string GetSoftwareLocation(IPublishedContent node)
      {
         node = node.Parent.Parent;

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


      public static bool SendEmail(string emailTo, IPublishedContent softwareItem, Extensions.Models.Aris.DownloadRequest downloadRequest)
      {
         bool success = false;

         IPublishedContent emailTemplate = Nodes.GetEmailTemplate("Software Download");

         if (emailTemplate != null)
         {
            var message = new MailMessage();

            message.From = new MailAddress(emailTemplate.GetPropertyValue<string>("emailFrom"));

            foreach (string emailToItem in emailTo.Split(new Char[] { ',', ';' }))
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
               body = body.Replace("[Software:SoftwareID]", softwareItem.GetPropertyValue<string>("softwareID"));
               body = body.Replace("[Software:Title]", softwareItem.GetPropertyValue<string>("title"));
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

                  body = body.Replace("[Download:" + prop.Name + "]", HttpContext.Current.Server.HtmlEncode(propValue));
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
