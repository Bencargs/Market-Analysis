using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using Serilog;

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
            var recipient = (await _emailTemplateProvider.GetEmailRecipients()).First(); // todo: enumerate this

            Log.Information($"Emailing recipient:{recipient.Name}");
            var (Body, Attachments) = await GetEmailBody(recipient, results);
            var message = CreateEmailMessage(recipient, Body, Attachments);

            var client = new SendGridClient(Configuration.SmtpApiKey);
            await client.SendEmailAsync(message);
        }

        private SendGridMessage CreateEmailMessage(RecipientDetails recipient, string content, List<Attachment> attachments)
        {
            var from = new EmailAddress("research@cbc.com", "CBC Market Analysis");
            var subject = $"Market Report {recipient.Date.ToShortDateString()}";
            var to = new EmailAddress(recipient.Email, recipient.Name);
            var plainTextContent = "";
            var htmlContent = content;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.AddAttachments(attachments);

            return msg;
        }

        private async Task<(string Body, List<Attachment> Attachments)> GetEmailBody(RecipientDetails recipient, IEnumerable<SimulationResult> results)
        {
            var profit = results.Sum(x => x.Worth) / results.Count();
            var recommendation = results.Any();

            var template = await _emailTemplateProvider.GetEmailTemplate();

            template = template.Replace(@"{date}", recipient.Date.ToString("dd/MM/yyyy"));
            template = template.Replace(@"{InvestorName}", recipient.Name);
            template = template.Replace(@"{InvestorNumber}", recipient.Number);
            template = template.Replace(@"{recommendation}", ToRecommendation(recommendation));
            template = template.Replace(@"{profit}", profit.ToString("C2"));

            var attachments = new List<Attachment>();
            template = await AddImageAsync(template, "logo", Configuration.LogoImagePath, attachments);
            template = await AddImageAsync(template, "website", Configuration.WorldImagePath, attachments);
            template = await AddImageAsync(template, "phone", Configuration.PhoneImagePath, attachments);
            template = await AddImageAsync(template, "email", Configuration.EmailImagePath, attachments);

            template = AddResults(template, results);

            return (template, attachments);
        }

        private string ToRecommendation(bool shouldBuy) => shouldBuy ? "Buy" : "Hold";

        private string AddResults(string template, IEnumerable<SimulationResult> simulationResults)
        {
            var results = new StringBuilder();
            foreach (var s in simulationResults)
            {
                results.Append("<tr>");
                results.Append($"<td>{s.Strategy}</td> <td>{ToRecommendation(s.ShouldBuy)}</td> <td>{s.Worth.ToString("C2")}</td>");
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
                using (Image image = Image.FromFile(path))
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
