using MarketAnalysis.Models;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public abstract class OptimisableStrategy : IStrategy
    {
        private DateTime? _lastOptimised;
        public abstract StrategyType StrategyType { get; }
        protected abstract TimeSpan OptimisePeriod { get; }
        public DateTime LatestDate { get; protected set; }

        public OptimisableStrategy(bool shouldOptimise)
        {
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?)null;
        }

        public bool ShouldOptimise()
        {
            if (_lastOptimised != null &&
                LatestDate > (_lastOptimised + OptimisePeriod))
            {
                _lastOptimised = LatestDate;
                return true;
            }
            return false;
        }

        public abstract IEnumerable<IStrategy> GetOptimisations();

        public abstract void SetParameters(IStrategy strategy);

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > LatestDate)
                LatestDate = data.Date;

            return ShouldBuy(data);
        }

        protected abstract bool ShouldBuy(MarketData data);
    }
}
