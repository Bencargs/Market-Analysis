using System.Collections.Generic;

namespace MarketAnalysis.Models.Reporting
{
    public class ReportPage
    {
        public string Body { get; protected set; }
        public List<Attachment> Attachments { get; private set; }

        public string GetRecommendation(bool shouldBuy) => shouldBuy ? "Buy" : "Hold";

        protected ReportPage()
        {
            Attachments = new List<Attachment>();
        }

        public ReportPage(string content)
            : this()
        {
            Body = content;
        }

        public ReportPage Replace(string field, string value)
        {
            Body = Body.Replace($"{{{field}}}", $"{value}");
            return this;
        }

        public ReportPage AddImage(string field, string path)
        {
            Replace(field, $"cid:{field}");
            Attachments.Add(new Attachment
            {
                Name = field,
                Content = Image.ToByteArray(path),
                AttachmentType = Attachment.Type.Image,
            });
            return this;
        }

        public ReportPage AddJsonFile(string name, byte[] content)
        {
            Attachments.Add(new Attachment
            {
                Name = name,
                Content = content,
                AttachmentType = Attachment.Type.Json,
            });
            return this;
        }
    }
}
