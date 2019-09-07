namespace MarketAnalysis.Models
{
    public class EmailTemplate
    {
        public EmailAddress[] Recipients { get; set; }
        public EmailAddress Sender { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
