using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy
{
    public class WeightedStrategy : IStrategy
    {
        private readonly double _threshold;
        private SimulationCache _cache;
        private Dictionary<IStrategy, double> _weights;

        private DateTime _latestDate;
        private DateTime? _lastOptimised;
        private static readonly TimeSpan OptimisePeriod = TimeSpan.FromDays(512);

        public WeightedStrategy(
            SimulationCache cache, 
            Dictionary<IStrategy, double> weights, 
            double threshold, 
            bool shouldOptimise = true)
        {
            _cache = cache;
            _weights = weights;
            _threshold = threshold;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?)null;
        }

        public WeightedStrategy(
            SimulationCache cache, 
            IStrategy[] strategies, 
            double threshold, 
            bool shouldOptimise = true)
            : this(cache, strategies.ToDictionary(k => k, v => 0d), threshold, shouldOptimise)
        {
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
            return Enumerable.Range(0, 100).SelectMany(x =>
            {
                return Enumerable.Range(1, 6).SelectMany(threshold =>
                {
                    var value = x / 100d;
                    var increment = 0.01d;
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

        public void SetParameters(IStrategy strategy)
        {
            _weights = ((WeightedStrategy)strategy)._weights;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            var sum = 0d;
            foreach (var s in _weights)
            {
                var result = _cache.GetOrCreate((s.Key, data.Date), null);
                var weight = Convert.ToDouble(result.ShouldBuy) * s.Value;
                sum += weight;
            }

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
