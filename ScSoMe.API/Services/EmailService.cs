using System.Net;
using System.Net.Mail;

namespace ScSoMe.API.Services
{
    public class EmailService
    {
        //public static async Task<MailMessage> SendMailAsync(string toEmail, List<string> bccs, string subject, string body, ILogger _logger, bool isBodyHtml = true)
        //{
        //    //SmtpClient smtpServer = new SmtpClient("smtp.office365.com", 587);
        //    SmtpClient smtpServer = new SmtpClient("email-smtp.eu-north-1.amazonaws.com", 587);
        //    smtpServer.EnableSsl = true;
        //    //smtpServer.Credentials = new NetworkCredential("info@startupcentral.dk", "UoPyQ3iKnIDC");
        //    smtpServer.Credentials = new NetworkCredential("AKIA5IOW3KCDOBL4EEX5", "BJtTrtHv0lCASKyPg9UyELuul1b913zLJlUrNMpw+zDB");

        //    MailMessage mail = new MailMessage();
        //    mail.From = new MailAddress("noreply@startupcentral.dk");

        //    var toString = "";
        //    if (!string.IsNullOrWhiteSpace(toEmail))
        //    {
        //        mail.To.Add(toEmail);
        //        toString += "to:" + toEmail + " ";
        //    }

        //    if (bccs != null)
        //    {
        //        toString += "bcc:";
        //        foreach (var bcc in bccs)
        //        {
        //            mail.Bcc.Add(bcc);
        //            toString += bcc + ",";
        //        }
        //    }

        //    mail.Subject = subject;

        //    if (isBodyHtml)
        //    {
        //        mail.Body = "<head><base href=\"https://www.startupcentral.dk/\"></head><body>" + body + "</body>";
        //        mail.IsBodyHtml = true;
        //    }
        //    else
        //    {
        //        mail.Body = body;
        //        mail.IsBodyHtml = false;
        //    }

        //    try
        //    {
        //        bool isProd = Environment.MachineName.Equals("startupVM");
        //        if (isProd)
        //        {
        //            // https://docs.microsoft.com/en-us/office365/servicedescriptions/exchange-online-service-description/exchange-online-limits#sending-limits  
        //            // 10,000 recipients per day, 30 messages per minute = 1800 msgs/hour = 43200 emails per day
        //            // await smtpServer.SendMailAsync(mail);
        //            _logger?.LogInformation("Email sent: " + toString);
        //        }
        //        else
        //        {
        //            _logger?.LogInformation("NOT isProd - email NOT sent: " + toString + Environment.NewLine + "Content:" + Environment.NewLine + body);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger?.LogError(e, $"Email sent error: + " + toString + Environment.NewLine + "Content:" + Environment.NewLine + body);
        //    }

        //    return mail;
        //}
    }
}
