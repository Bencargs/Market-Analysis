using System;

namespace MarketAnalysis.Models.ApiData
{
    public class FileMarketData
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Delta { get; set; }
        public decimal Volume { get; set; }
        public decimal Spread { get; set; }
    }
}
