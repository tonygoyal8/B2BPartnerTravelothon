using System;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace B2BPartnerTravelothon.Repository.Email
{
    public class EMail
    {

        #region Private Fields

        private static string FromAddress;
        private static string strSmtpClient;
        private static string UserID;
        private static string Password;
        private static string SMTPPort;
        private static bool bEnableSSL;

        #endregion

        #region Interface Implementation

        public async Task SendAsync(IdentityMessage message, string CC = "", string BCC = "",bool hideFooter=false)
        {
            await configSendGridasync(message, CC, BCC, hideFooter);
        }

        #endregion

        #region Send Email Method
        public async Task configSendGridasync(IdentityMessage message, string CC = "", string BCC = "", bool hideFooter = false)
        {
            GetMailData();
            var MailMessage = new MailMessage();
            MailMessage.From = new MailAddress(FromAddress,"Travelothon");
            MailMessage.Priority = MailPriority.High;
            MailMessage.To.Add(message.Destination);
            MailMessage.Subject = message.Subject;
            MailMessage.IsBodyHtml = true;
            MailMessage.Body = message.Body;
            if (!String.IsNullOrEmpty(CC))
            {
                MailMessage.CC.Add(CC);
            }
            if (!String.IsNullOrEmpty(BCC))
            {
                MailMessage.Bcc.Add(BCC);
            }


            SmtpClient SmtpClient = new SmtpClient();
            SmtpClient.Host = strSmtpClient;
            SmtpClient.EnableSsl = bEnableSSL;
            SmtpClient.Port = Int32.Parse(SMTPPort);
            SmtpClient.Credentials = new System.Net.NetworkCredential(UserID, Password);
            LinkedResource LinkedImage = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~/assets/TravelothonEmail/TravelothonEmailLogo.jpg"));
            LinkedImage.ContentType = new ContentType(MediaTypeNames.Image.Jpeg);
            LinkedImage.ContentId = "Travelothon";
            if (!hideFooter)
            {
                message.Body += "<br/><br/>For any assistance, please email our customer support:<a href='b2bsupport@travelothon.in'>b2bsupport@travelothon.in</a>.<br/><br/>Thanks,<br/><br/>Travelothon | Travel Services Made Easy";
                string footer = "";
                footer = "<a href='www.travelothon.in'>www.travelothon.in</a><br/><br/>";
                footer += "<p style='font-family: Calibri;font-size:12px'><b>Important:</b>Please do not reply to this email.It will be sent to an email account that is unattended. Please add no-reply@travelothon.in to your contact list and address book to ensure that you receive communication related to your transactions from <a href='www.travelothon.in'>Travelothon</a>.</p>";
                footer += "<br/><br/><p align='center'><a href='https://www.facebook.com/Travelothon'>Facebook</a> | <a href='https://www.linkedin.com/in/travelothon'>Linkedin</a></p>";

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body + "<br/><br/><a href='www.travelothon.in'><img src=cid:Travelothon></a><br/><br/>" + footer, null, "text/html");
                htmlView.LinkedResources.Add(LinkedImage);
                MailMessage.AlternateViews.Add(htmlView);
            }
            else
            {
               var footer = "<br/><br/><br/><p style='font-family: Calibri;font-size:12px'><b>Important:</b>Please do not reply to this email.It will be sent to an email account that is unattended. Please add no-reply@travelothon.in to your contact list and address book to ensure that you receive communication related to your transactions from <a href='www.travelothon.in'>Travelothon</a>.</p>";
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message.Body+ footer, null, "text/html");
                htmlView.LinkedResources.Add(LinkedImage);
                MailMessage.AlternateViews.Add(htmlView);
            }



            try
            {
                 SmtpClient.Send(MailMessage);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                int j = 0;
                for (int i = 0; i <= ex.InnerExceptions.Length && j < 3; i++)
                {
                    SmtpStatusCode status = ex.StatusCode;
                    if ((status == SmtpStatusCode.MailboxBusy) || (status == SmtpStatusCode.MailboxUnavailable))
                    {
                        System.Threading.Thread.Sleep(5000);
                        SmtpClient.Send(MailMessage);
                    }
                    j++;
                }
            }
            catch (Exception ex)
            {

            }

        }

        #endregion

        #region Get Email provider data From Web.config file
        private static void GetMailData()
        {
            FromAddress = System.Configuration.ConfigurationManager.AppSettings.Get("FromAddress");
            strSmtpClient = System.Configuration.ConfigurationManager.AppSettings.Get("SmtpClient");
            UserID = System.Configuration.ConfigurationManager.AppSettings.Get("UserID");
            Password = System.Configuration.ConfigurationManager.AppSettings.Get("Password");
            //ReplyTo = System.Configuration.ConfigurationManager.AppSettings.Get("ReplyTo");
            SMTPPort = System.Configuration.ConfigurationManager.AppSettings.Get("SMTPPort");
            if ((System.Configuration.ConfigurationManager.AppSettings.Get("EnableSSL") == null))
            {
            }
            else
            {
                if ((System.Configuration.ConfigurationManager.AppSettings.Get("EnableSSL").ToUpper() == "YES"))
                {
                    bEnableSSL = true;
                }
                else
                {
                    bEnableSSL = false;
                }
            }
        }


        #endregion

    }
}