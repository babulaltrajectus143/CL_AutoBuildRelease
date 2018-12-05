using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CL_AutoBuildRelease
{
    public static class MailServer
    {
        public static bool SendEmail(string sharedURL, string sharedPassword, string Foldername)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(ConfigurationSettings.AppSettings["SMTPUsername"].ToString());
                mail.To.Add(ConfigurationSettings.AppSettings["Recipients"].ToString());
                mail.Subject = "Shared Link(" + Foldername + ")";
                mail.IsBodyHtml = true;
                mail.Body = @"The Shared Link for " + Foldername + @" is {" + sharedURL + @"}.Please use password {" + sharedPassword + @"}
                                                to download this file.
                                Reagrds,
                                Shubham Goyal.
                                Espline LLC
                            ";
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationSettings.AppSettings["SMTPUsername"].ToString()
                                             , ConfigurationSettings.AppSettings["SMTPPassword"].ToString());
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static bool SendHTMLMail(string sharedURL, string sharedPassword, string Foldername , string productname , string filelist , string uploadedProject)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(ConfigurationSettings.AppSettings["SMTPUsername"].ToString());
                mail.To.Add(ConfigurationSettings.AppSettings["Recipients"].ToString());
                mail.Subject = "Shared Link(" + Foldername + ")";
                mail.IsBodyHtml = true;
                string strTemplate = ReadTemplate();
                mail.Body = strTemplate.Replace("{Password}", sharedPassword).Replace("{Product}", productname).Replace("{Project}", uploadedProject).Replace("{ShareFileLink}", sharedURL).Replace("{FileList}", filelist);
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationSettings.AppSettings["SMTPUsername"].ToString(), ConfigurationSettings.AppSettings["SMTPPassword"].ToString());
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string ReadTemplate()
        {
            string[] strfiles = System.IO.Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Espline_mail_template.html");
            if (strfiles.Count() > 0)
                return string.Join(" ", System.IO.File.ReadAllLines(strfiles[0]));
            else
                return "";
        }
    }
}
