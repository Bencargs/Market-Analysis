using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class MovingAverageParameters : IParameters
    {
        public int Window { get; set; }
        public double Threshold { get; set; }
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(1024);
    }
}
