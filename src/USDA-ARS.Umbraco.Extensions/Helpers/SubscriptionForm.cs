using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class SubscriptionForm
    {
        public static bool SendEmail(string emailFrom, string emailTo, string subject, string body, bool isHtmlEmail = false)
        {
            bool success = false;

            var message = new MailMessage();

            message.From = new MailAddress(emailFrom.Trim());

            foreach (string emailToItem in emailTo.Split(';'))
            {
                message.To.Add(new MailAddress(emailToItem.Trim()));
            }

            message.Subject = subject.Trim();
            message.Body = body;
            message.IsBodyHtml = isHtmlEmail;

            using (var smtp = new SmtpClient())
            {
                smtp.Send(message);

                success = true;
            }

            return success;
        }
    }
}
