using System;

namespace MarketAnalysis.Models
{
    public class Investor
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal DailyFunds { get; set; }
        public int OrderDelayDays { get; set; }
        public decimal OrderBrokerage { get; set; }
    }
}
