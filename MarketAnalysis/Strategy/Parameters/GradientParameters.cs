using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class GradientParameters : IParameters
    {
        public int Window { get; set; }
        public decimal Threshold { get; set; }
        public TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(256);
    }
}
