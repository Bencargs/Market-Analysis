using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class EmailCommunicationService : ICommunicationService<SimulationResult>
    {
        private readonly IProvider _emailTemplateProvider;

        public EmailCommunicationService(IProvider emailTemplateProvider)
        {
            _emailTemplateProvider = emailTemplateProvider;
        }

        public async Task SendCommunication(IEnumerable<SimulationResult> results)
        {
            string[] scopes = { GmailService.Scope.GmailCompose, GmailService.Scope.GmailSend };

            UserCredential credential;
            var tokenPath = "token.json";
            using (var stream = new FileStream(@"C:\Source\credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true));
            }

            var mail = new MailMessage();
            mail.Subject = "subject Test";
            mail.Body = await GetEmailBody();
            mail.From = new MailAddress("research@cbc.com");
            mail.IsBodyHtml = true;
            //var attImg = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Logo.png";
            //var attachement = new Attachment(attImg);
            //attachement.ContentId = "logo";
            ////attachement.ContentDisposition.Inline = true;
            //mail.Attachments.Add(attachement);
            mail.To.Add(new MailAddress("benjamin.d.cargill@gmail.com"));
            var mimeMessage = MimeMessage.CreateFromMailMessage(mail);

            //var body = await GetEmailBody();
            //string plainText = "To: benjamin.d.cargill@gmail.com\r\n" +
            //                   "Subject: subject Test\r\n" +
            //                   "Content-Type: text/html; charset=us-ascii\r\n\r\n" +
            //                   $"{body}";

            var newMsg = new Message
            {
                //Raw = Base64UrlEncode(plainText.ToString())
                Raw = Base64UrlEncode(mimeMessage.ToString())
            };

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Market Analysis"
            });
            service.Users.Messages.Send(newMsg, "me").Execute();
            return;

            // todo: replace with Google OAuth authentication below -
            //var secrets = new ClientSecrets
            //{
            //    ClientId = "abc",
            //    ClientSecret = "abc"
            //};

            //string user = "";
            //var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
            //    new[] { GmailService.Scope.MailGoogleCom }, user, CancellationToken.None);

            //var template = await _emailTemplateProvider.GetEmailTemplate();
            //var message = CreateEmailMessage(template);
            //using (var emailClient = new SmtpClient())
            //{
            //    emailClient.Connect(Configuration.SmtpServer, Configuration.SmtpPort);
            //    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
            //    emailClient.Authenticate(Configuration.SmtpUsername, Configuration.SmtpPassword);
            //    await emailClient.SendAsync(message);
            //    await emailClient.DisconnectAsync(true);
            //}
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        //public async static Task<string> Base64ImageEncode(string path)
        //{
        //    return await Task.Run(() =>
        //    {
        //        using (var image = Image.FromFile(path))
        //        using (var stream = new MemoryStream())
        //        {
        //            image.Save(stream, image.RawFormat);
        //            var bytes = stream.ToArray();
        //            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        //        }
        //    });
        //}

        public async static Task<string> GetUTF8Image(string path)
        {
            return await Task.Run(() =>
            {
                using (var image = Image.FromFile(path))
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, image.RawFormat);
                    var bytes = stream.ToArray();
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
            });
        }

        public async static Task<string> GetEmailBody()
        {
            var templatePath = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Template.html";
            var template = await File.ReadAllTextAsync(templatePath);

            var logoImagePath = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Logo.png";

            //var logo = await Base64ImageEncode(logoImagePath);

            //var websiteImagePath = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\World.png";
            //var website = await Base64ImageEncode(websiteImagePath);

            //var phoneImagePath = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Phone.png";
            //var phone = await Base64ImageEncode(phoneImagePath);

            //var emailImagePath = @"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Email.png";
            //var email = await Base64ImageEncode(emailImagePath);

            template = template
                //.Replace(@"{logo}", logo)
                .Replace(@"{logo}", "cid:logo")
                //.Replace(@"{website}", website)
                //.Replace(@"{phone}", phone)
                //.Replace(@"{email}", email)
                .Replace(@"{date}", DateTime.Now.ToString("dd/MM/yyyy"));

            return template.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private MimeMessage CreateEmailMessage(EmailTemplate template)
        {
            var message = new MimeMessage();
            message.To.AddRange(template.Recipients.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.Add(new MailboxAddress(template.Sender.Name, template.Sender.Address));
            message.Subject = template.Subject;
            var builder = new BodyBuilder();
            builder.TextBody = template.Content;
            builder.Attachments.Add(@"C:\Source\MarketAnalysis\MarketAnalysis\Resources\Template.pdf");
            message.Body = builder.ToMessageBody();
            //message.Body = new TextPart(TextFormat.Html)
            //{
            //    Text = template.Content
            //};
            

            return message;
        }
    }
}
