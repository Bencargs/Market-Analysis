using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy
    {
        private decimal _threshold;
        private static TimeSpan OptimisePeriod = TimeSpan.FromDays(512);
        private DateTime _latestDate;
        private DateTime? _lastOptimised;

        public object Key => _threshold;

        public DeltaStrategy(decimal threshold, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?)null;
        }

        public bool ShouldOptimise()
        {
            if (_lastOptimised != null &&
                _latestDate > (_lastOptimised + OptimisePeriod))
            {
                _lastOptimised = _latestDate;
                return true;
            }
            return false;
        }

        public IEnumerable<IStrategy> GetOptimisations()
        {
            return Enumerable.Range(1, 100).Select(x =>
            {
                var parameter = (decimal)x / 1000;
                return new DeltaStrategy(parameter, false);
            });
        }

        public void SetParameters(IStrategy strategy)
        {
            _threshold = ((DeltaStrategy)strategy)._threshold;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            return data.Delta < _threshold;
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
