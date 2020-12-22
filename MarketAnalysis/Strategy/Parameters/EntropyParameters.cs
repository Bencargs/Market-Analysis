using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class EntropyParameters : IParameters
    {
        public double Threshold { get; set; }
        public int Window { get; set; }
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(512);
    }
}
