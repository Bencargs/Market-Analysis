using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class RelativeStrengthParameters : IParameters
    {
        public int Threshold { get; set; }
        public int[] TestSet { get; set; }
        public TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(256);
    }
}
