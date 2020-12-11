using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class LinearRegressionParameters : IParameters
    {
        public int Lookback { get; set; }
        public TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(128);
    }
}
