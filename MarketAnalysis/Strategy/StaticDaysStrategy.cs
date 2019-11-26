using MarketAnalysis.Models;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy
    {
        private readonly Dictionary<DateTime, bool> _buyDates;

        public object Key => _buyDates;

        public StaticDatesStrategy(Dictionary<DateTime, bool> buyDates)
        {
            _buyDates = buyDates;
        }

        public bool ShouldOptimise()
        {
            return false;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return new IStrategy[0];
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            return _buyDates[data.Date];
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
