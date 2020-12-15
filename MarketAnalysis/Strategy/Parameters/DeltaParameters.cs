using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class DeltaParameters : IParameters
    {
        public decimal Threshold { get; set; }
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(128);
    }
}
