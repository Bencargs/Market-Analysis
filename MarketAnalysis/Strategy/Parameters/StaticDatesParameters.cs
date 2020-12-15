using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy.Parameters
{
    public class StaticDatesParameters : IParameters
    {
        public Dictionary<DateTime, bool> BuyDates;
        public int Identifier;
        public TimeSpan? OptimisePeriod { get; } = null;
    }
}
