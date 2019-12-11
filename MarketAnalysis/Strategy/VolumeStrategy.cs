﻿using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : IStrategy
    {
        private int _threshold;
        private decimal _previousVolume;
        private static TimeSpan OptimisePeriod = TimeSpan.FromDays(512);
        private DateTime _latestDate;
        private DateTime? _lastOptimised;

        public object Key => _threshold;

        public VolumeStrategy(int threshold, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?) null;
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
            return Enumerable.Range(1, 800).Select(x => 
                new VolumeStrategy(x, false));
        }

        public void SetParameters(IStrategy strategy)
        {
            _threshold = ((VolumeStrategy)strategy)._threshold;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            var shouldBuy = _previousVolume != 0 && 
                (data.Volume / _previousVolume) < _threshold;
            
            _previousVolume = data.Volume;
            return shouldBuy;
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
