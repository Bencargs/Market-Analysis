using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class OddsParameters : IParameters
    {
        public decimal MinPrice { get; set; }
        public int WaitTime { get; set; }
        public int MaxWaitTime { get; set; }
        
        public TimeSpan? OptimisePeriod { get; set; } = TimeSpan.FromDays(512);
    }
}
