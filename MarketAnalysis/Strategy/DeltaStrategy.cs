using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy
    {
        private readonly decimal _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private DateTime _latestDate;

        public object Key => _threshold;

        public DeltaStrategy(decimal threshold, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _shouldOptimise = shouldOptimise;
        }

        public bool ShouldOptimise()
        {
            var count = MarketDataCache.Instance.Count;
            return _shouldOptimise &&
                   count % OptimisePeriod == 0;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return Enumerable.Range(1, 200).Select(x =>
            {
                var parameter = (decimal)x / 1000;
                return new DeltaStrategy(parameter, false);
            });
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            return Math.Abs(data.Delta) < _threshold;
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as DeltaStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
