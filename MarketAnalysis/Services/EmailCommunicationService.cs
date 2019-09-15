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
using System.IO;
using System.Linq;
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

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Market Analysis"
            });

            string plainText = "To: TEST@gmail.com\r\n" +
                               "Subject: subject Test\r\n" +
                               "Content-Type: text/html; charset=us-ascii\r\n\r\n" +
                               "<h1>Body Test </h1>";

            var newMsg = new Message
            {
                Raw = Base64UrlEncode(plainText.ToString())
            };
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

            var template = await _emailTemplateProvider.GetEmailTemplate();
            var message = CreateEmailMessage(template);
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect(Configuration.SmtpServer, Configuration.SmtpPort);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(Configuration.SmtpUsername, Configuration.SmtpPassword);
                await emailClient.SendAsync(message);
                await emailClient.DisconnectAsync(true);
            }
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
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
