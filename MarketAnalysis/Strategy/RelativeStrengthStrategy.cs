using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : IStrategy
    {
        private int _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private DateTime _latestDate;

        public object Key => _threshold;

        public RelativeStrengthStrategy(int threshold, bool shouldOptimise = true)
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
            return Enumerable.Range(1, 101).Select(x =>
                new RelativeStrengthStrategy(x, false));
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            var batch = MarketDataCache.Instance.GetLastSince(_latestDate, _threshold).ToArray();
            if (batch.Count() < 3)
                return false;

            var strength = GetRelativeStrength(data.Price, batch);

            // todo: these parameters should be derived from self optimisation
            return new[] { 0, 3, 5, 6 }.Contains(strength);
        }

        private int GetRelativeStrength(decimal price, MarketData[] data)
        {
            var min = data.Min(y => y.Price);
            var max = data.Max(y => y.Price);
            var range = max - min;
            if (range == 0)
                return 100;

            var adjustedPrice = price - min;
            return Convert.ToInt32(adjustedPrice / range * 100);
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as RelativeStrengthStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
