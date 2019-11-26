using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : IStrategy
    {
        private readonly int _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private DateTime _latestDate;

        public object Key => _threshold;

        public VolumeStrategy(int threshold, bool shouldOptimise = true)
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
            return Enumerable.Range(1, 800).Select(x => new VolumeStrategy(x, false));
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            return data.Volume < _threshold;
        }

        public override bool Equals(object obj)
        {
            return Key.Equals((obj as VolumeStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
