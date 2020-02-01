using System.ComponentModel;

namespace MarketAnalysis.Models.Reporting
{
    public class Attachment
    {
        public string Name { get; set; }
        public Type AttachmentType { get; set; }
        public byte[] Content { get; set; }

        public enum Type
        {
            None = 0,

            [Description("image/png")]
            Image,

            [Description("application/json")]
            Json
        }
    }
}
