using System;

namespace MarketAnalysis.Models
{
    public class SimulationState
    {
        public DateTime Date { get; set; }
        public decimal Funds { get; set; }
        public decimal Shares { get; set; }
        public bool ShouldBuy { get; set; }
        public decimal SharePrice { get; set; }
        public int BuyCount { get; set; }
        public decimal Worth => Funds + (Shares * SharePrice);
    }
}
