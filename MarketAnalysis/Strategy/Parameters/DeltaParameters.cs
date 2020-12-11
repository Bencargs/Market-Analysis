using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public interface IParameters
    {
        public TimeSpan OptimisePeriod { get; }
    }

    public class DeltaParameters : IParameters
    {
        public decimal Threshold { get; set; }
        public TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(128);
    }
}
