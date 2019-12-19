using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : OptimisableStrategy
    {
        private decimal _threshold;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(512);

        public DeltaStrategy(decimal threshold, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _threshold = threshold;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return Enumerable.Range(1, 100).Select(x =>
            {
                var parameter = (decimal)x / 1000;
                return new DeltaStrategy(parameter, false);
            });
        }

        public override void SetParameters(IStrategy strategy)
        {
            _threshold = ((DeltaStrategy)strategy)._threshold;
        }

        public override bool ShouldAddFunds()
        {
            return true;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            return data.Delta < _threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DeltaStrategy strategy))
                return false;

            return strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode();
        }
    }
}
