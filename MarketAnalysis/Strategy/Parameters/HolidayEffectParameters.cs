using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class HolidayEffectParameters : IParameters
    {
        public TimeSpan? OptimisePeriod { get; } = null;
    }
}
