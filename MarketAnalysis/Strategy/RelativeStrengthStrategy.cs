﻿using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : IStrategy
    {
        private int _threshold;
        private DateTime _latestDate;
        private DateTime? _lastOptimised;
        private static readonly TimeSpan OptimisePeriod = TimeSpan.FromDays(512);

        public RelativeStrengthStrategy(int threshold, bool shouldOptimise = true)
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
            return Enumerable.Range(1, 101).Select(x =>
                new RelativeStrengthStrategy(x, false));
        }

        public void SetParameters(IStrategy strategy)
        {
            _threshold = ((RelativeStrengthStrategy)strategy)._threshold;
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
            if (!(obj is RelativeStrengthStrategy strategy))
                return false;

            return strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode();
        }
    }
}
