using MarketAnalysis.Models;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy
    {
        public int Identifier { get; set; }

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

        public IEnumerable<IStrategy> GetOptimisations()
        {
            return new IStrategy[0];
        }

        public void SetParameters(IStrategy strategy)
        {
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
            return (obj as StaticDatesStrategy)?.Identifier == Identifier;
        }

        public override int GetHashCode()
        {
            return Identifier;
        }
    }
}
