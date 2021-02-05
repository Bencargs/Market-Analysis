using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy.Parameters
{
    public class ProbabilityParameters : IParameters
    {
        public int Threshold { get; set; }
        public Dictionary<int, List<int>> Histogram { get; set; } = new();
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(1024);
    }
}
