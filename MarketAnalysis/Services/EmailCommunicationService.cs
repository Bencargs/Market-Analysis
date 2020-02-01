using MarketAnalysis.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Attachment = SendGrid.Helpers.Mail.Attachment;
using MarketAnalysis.Models.Reporting;

namespace MarketAnalysis.Services
{
    public class EmailCommunicationService : ICommunicationService
    {
        private readonly ReportProvider _emailTemplateProvider;
        private readonly Converter<ReportPage, (string html, List<Attachment> attachments)> _emailConverter;

        public EmailCommunicationService(ReportProvider emailTemplateProvider)
        {
            _emailTemplateProvider = emailTemplateProvider;
            _emailConverter = new Converter<ReportPage, (string html, List<Attachment> attachments)>(TemplateToEmail);
        }

        public async Task SendCommunication(IResultsProvider resultsProvider)
        {
            var client = new SendGridClient(Configuration.SmtpApiKey);
            var recipients = _emailTemplateProvider.GetEmailRecipients();
            foreach (var recipient in recipients)
            {
                Log.Information($"Emailing recipient:{recipient.Name}");
                var report = _emailTemplateProvider.GenerateReports(recipient, resultsProvider);
                var (html, attachments) = _emailConverter(report.Summary);
                var message = CreateEmailMessage(recipient, html, attachments);

                await client.SendEmailAsync(message);
            }
        }

        private SendGridMessage CreateEmailMessage(RecipientDetails recipient, string content, List<Attachment> attachments)
        {
            var from = new EmailAddress("research@cbc.com", "CBC Market Analysis");
            var subject = $"Market Report {recipient.Date.ToString("dd MMM yyyy")}";
            var to = new EmailAddress(recipient.Email, recipient.Name);
            var plainTextContent = "";
            var htmlContent = content;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.AddAttachments(attachments);

            return msg;
        }

        private static (string html, List<Attachment> attachments) TemplateToEmail(ReportPage template)
        {
            var attachments = new List<Attachment>();
            foreach (var a in template.Attachments)
            {
                switch (a.AttachmentType)
                {
                    case Models.Reporting.Attachment.Type.Image:
                        attachments.Add(new Attachment
                        {
                            Content = Convert.ToBase64String(a.Content),
                            Type = a.AttachmentType.GetDescription(),
                            Filename = $"{a.Name}.png",
                            Disposition = "inline",
                            ContentId = $"{a.Name}"
                        });
                        break;
                    case Models.Reporting.Attachment.Type.Json:
                        attachments.Add(new Attachment
                        {
                            Content = Convert.ToBase64String(a.Content),
                            Type = a.AttachmentType.GetDescription(),
                            Filename = $"{a.Name}.json",
                            Disposition = "attachment"
                        });
                        break;
                }
            }
            return (template.Body, attachments);
        }
    }
}
