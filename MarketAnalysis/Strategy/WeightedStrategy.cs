using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Simulation;
using ShellProgressBar;

namespace MarketAnalysis.Strategy
{
    public class WeightedStrategy : OptimisableStrategy, IAggregateStrategy
    {
        private readonly double _threshold;
        private readonly SimulationCache _cache;
        private Dictionary<IStrategy, double> _weights;
        public override StrategyType StrategyType { get; } = StrategyType.Weighted;
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

        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
        {
            var potentials = Enumerable.Range(0, 100).SelectMany(x =>
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

            var searcher = new LinearSearch(simulator, potentials, progress);
            simulator.RemoveCache(potentials.Except(new[] { this }));
            return searcher.Maximum(LatestDate);
        }

        protected override void SetParameters(IStrategy strategy)
        {
            _weights = ((WeightedStrategy)strategy)._weights;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var sum = 0d;
            foreach (var s in _weights)
            {
                _cache.TryGet((s.Key, data.Date), out var result);
                var weight = Convert.ToDouble(result.ShouldBuy) * s.Value;
                sum += weight;
            }

            return sum > _threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WeightedStrategy strategy))
                return false;

            var self = _weights.Values.ToArray();
            var other = strategy._weights.Values.ToArray();
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] != other[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return _weights.GetHashCode();
        }
    }
}
