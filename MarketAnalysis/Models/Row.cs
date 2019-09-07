using System;

namespace MarketAnalysis.Models
{
    public class Row
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Delta { get; set; }
        public decimal Volume { get; set; }
    }
}
