using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy.Parameters
{
    public class WeightedParameters : IParameters
    {
        public double Threshold { get; set; }
        public Dictionary<IStrategy, double> Weights { get; set; } = new();
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(512);
    }
}
