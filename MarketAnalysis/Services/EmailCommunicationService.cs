using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using Serilog;
using Newtonsoft.Json;

namespace MarketAnalysis.Services
{
    public class EmailCommunicationService : ICommunicationService
    {
        private readonly IProvider _emailTemplateProvider;

        public EmailCommunicationService(IProvider emailTemplateProvider)
        {
            _emailTemplateProvider = emailTemplateProvider;
        }

        public async Task SendCommunication(IResultsProvider resultsProvider)
        {
            var client = new SendGridClient(Configuration.SmtpApiKey);
            var recipients = (await _emailTemplateProvider.GetEmailRecipients());
            foreach (var recipient in recipients)
            {
                Log.Information($"Emailing recipient:{recipient.Name}");
                var (Body, Attachments) = await GetEmailBody(recipient, resultsProvider);
                var message = CreateEmailMessage(recipient, Body, Attachments);

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

        private async Task<(string Body, List<Attachment> Attachments)> GetEmailBody(RecipientDetails recipient, IResultsProvider resultsProvider)
        {
            var totalProfit = resultsProvider.TotalProfit();
            var shouldBuy = resultsProvider.ShouldBuy();

            var template = await _emailTemplateProvider.GetEmailTemplate();

            template = template.Replace(@"{date}", recipient.Date.ToString("dd/MM/yyyy"));
            template = template.Replace(@"{InvestorName}", recipient.Name);
            template = template.Replace(@"{InvestorNumber}", recipient.Number);
            template = template.Replace(@"{recommendation}", ToRecommendation(shouldBuy));
            template = template.Replace(@"{profit}", totalProfit.ToString("C2"));

            var attachments = new List<Attachment>();
            template = await AddImageAsync(template, "logo", Configuration.LogoImagePath, attachments);
            template = await AddImageAsync(template, "website", Configuration.WorldImagePath, attachments);
            template = await AddImageAsync(template, "phone", Configuration.PhoneImagePath, attachments);
            template = await AddImageAsync(template, "email", Configuration.EmailImagePath, attachments);

            AddDetailedResults(resultsProvider, attachments);

            template = AddResults(template, resultsProvider);

            return (template, attachments);
        }

        private void AddDetailedResults(IResultsProvider resultsProvider, List<Attachment> attachments)
        {
            // todo: custom PDF generation for each strategy type
            var json = JsonConvert.SerializeObject(resultsProvider.GetResults());
            var bytes = Encoding.ASCII.GetBytes(json);
            attachments.Add(new Attachment
            {
                Content = Convert.ToBase64String(bytes),
                Type = "application/json",
                Filename = "results.json",
                Disposition = "attachment"
            });
        }

        private string ToRecommendation(bool shouldBuy) => shouldBuy ? "Buy" : "Hold";

        private string AddResults(string template, IResultsProvider resultsProvider)
        {
            var results = new StringBuilder();
            foreach (var s in resultsProvider.GetResults())
            {
                results.Append("<tr>");
                results.Append($"<td>{s.StrategyName}</td>");
                results.Append($"<td>{ToRecommendation(s.ShouldBuy)}</td>");
                results.Append($"<td>{s.ProfitYTD.ToString("C2")}</td>");
                results.Append($"<td>{s.ProfitTotal.ToString("C2")}</td>");
                results.Append("</tr>");
            }
            return template.Replace(@"{results}", results.ToString());
        }

        private async Task<string> AddImageAsync(string template, string tag, string path, List<Attachment> attachments)
        {
            var content = await Base64ImageEncode(path);
            attachments.Add(new Attachment
            {
                Content = content,
                Type = "image/png",
                Filename = $"{tag}.png",
                Disposition = "inline",
                ContentId = $"{tag}"
            });
            return template.Replace($"{{{tag}}}", $"cid:{tag}");
        }

        private async static Task<string> Base64ImageEncode(string path)
        {
            return await Task.Run(() =>
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, image.RawFormat);
                    byte[] imageBytes = stream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            });
        }
    }
}
