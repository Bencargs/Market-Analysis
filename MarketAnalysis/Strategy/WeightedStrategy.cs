using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Staking;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class WeightedStrategy : IStrategy, IAggregateStrategy, IEquatable<WeightedStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IStakingService _stakingService;
        private readonly ISimulationCache _simulationCache;
        private WeightedParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Weighted;

        public WeightedStrategy(
            ISimulationCache simulationCache,
            IStakingService stakingService,
            ISearcher searcher,
            WeightedParameters parameters)
        {
            _simulationCache = simulationCache;
            _stakingService = stakingService;
            _searcher = searcher;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            _stakingService.Evaluate(fromDate, endDate);

            var potentials = Enumerable.Range(0, 100).SelectMany(x =>
            {
                return Enumerable.Range(1, 6).SelectMany(threshold =>
                {
                    var value = x / 100d;
                    const double increment = 0.001d;
                    return Enumerable.Range(0, _parameters.Weights.Count).Select(y =>
                    {
                        var newWeights = _parameters.Weights.Select((w, j) =>
                        {
                            var allocation = j == y ? (value + increment) : value;
                            return (strategy: w.Key, allocation);
                        }).ToDictionary(k => k.strategy, v => v.allocation);

                        return new WeightedParameters{ Threshold = threshold, Weights = newWeights };
                    });
                });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (WeightedParameters) optimum;
        }
        public bool ShouldBuy(MarketData data)
        {
            var sum = 0d;
            foreach (var (strategy, w) in _parameters.Weights)
            {
                if (!_simulationCache.TryGet((strategy, data.Date), out var shouldBuy))
                    continue;
                    
                var weight = Convert.ToDouble(shouldBuy) * w;
                sum += weight;
            }

            return sum > _parameters.Threshold;
        }

        public decimal GetStake(DateTime today, decimal totalFunds)
        {
            return _stakingService.GetStake(today, totalFunds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(WeightedStrategy)) return false;

            return Equals(obj as WeightedStrategy);
        }

        public bool Equals(WeightedStrategy strategy)
        {
            if (_parameters.Threshold != strategy._parameters.Threshold)
                return false;

            var self = _parameters.Weights.Values.ToArray();
            var other = strategy._parameters.Weights.Values.ToArray();
            return !self.Where((t, i) => Math.Abs(t - other[i]) > 0.00001).Any();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold, _parameters.Weights);
        }
    }
}
