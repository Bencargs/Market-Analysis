namespace MarketAnalysis.Models
{
    public class SimulationState
    {
        public decimal Funds { get; set; }
        public decimal Shares { get; set; }
        public bool ShouldBuy { get; set; }
        public decimal LatestPrice { get; set; }
        public int BuyCount { get; set; }
    }
}
