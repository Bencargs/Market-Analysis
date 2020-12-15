using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public interface IParameters
    {
        public TimeSpan? OptimisePeriod { get; }
    }
}
