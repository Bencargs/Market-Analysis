﻿using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy
{
    public class WeightedStrategy : OptimisableStrategy
    {
        private readonly double _threshold;
        private readonly SimulationCache _cache;
        private Dictionary<IStrategy, double> _weights;
        private static Dictionary<DateTime, double> _confidence = new Dictionary<DateTime, double>();
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(512);

        public WeightedStrategy(
            SimulationCache cache, 
            Dictionary<IStrategy, double> weights, 
            double threshold, 
            bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _cache = cache;
            _weights = weights;
            _threshold = threshold;
        }

        public WeightedStrategy(
            SimulationCache cache, 
            IStrategy[] strategies, 
            double threshold, 
            bool shouldOptimise = true)
            : this(cache, strategies.ToDictionary(k => k, v => 0d), threshold, shouldOptimise)
        {
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return Enumerable.Range(0, 100).SelectMany(x =>
            {
                return Enumerable.Range(1, 6).SelectMany(threshold =>
                {
                    var value = x / 100d;
                    var increment = 0.001d;
                    return Enumerable.Range(0, _weights.Count).Select(y =>
                    {
                        var newWeights = _weights.Select((w, j) =>
                        {
                            var allocation = j == y ? (value + increment) : value;
                            return (strategy: w.Key, allocation);
                        }).ToDictionary(k => k.strategy, v => v.allocation);

                        return new WeightedStrategy(_cache, newWeights, threshold, false);
                    });
                });
            });
        }

        public override void SetParameters(IStrategy strategy)
        {
            _weights = ((WeightedStrategy)strategy)._weights;
        }

        public override bool ShouldAddFunds()
        {
            return true;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var sum = 0d;
            foreach (var s in _weights)
            {
                var result = _cache.GetOrCreate((s.Key, data.Date), null);
                var weight = Convert.ToDouble(result.ShouldBuy) * s.Value;
                sum += weight;
            }

            if (!_confidence.ContainsKey(data.Date))
                _confidence.Add(data.Date, sum);

            return sum > _threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WeightedStrategy strategy))
                return false;

            return _weights.All(entry => 
                strategy._weights.TryGetValue(entry.Key, out var value) && 
                value.Equals(entry.Value));
        }

        public override int GetHashCode()
        {
            return _weights.GetHashCode();
        }
    }
}