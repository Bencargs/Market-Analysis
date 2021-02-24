using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class ProbabilityStrategy : IStrategy, IEquatable<ProbabilityStrategy>
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISearcher _searcher;
        private ProbabilityParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Probability;

        public ProbabilityStrategy(
            IMarketDataCache marketDataCache,
            ISearcher searcher,
            ProbabilityParameters parameters)
        {
            _marketDataCache = marketDataCache;
            _searcher = searcher;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            Dictionary<int, List<int>> histogram = new();
            var history = _marketDataCache.TakeUntil(toDate).ToArray();
            for (var i = 1; i < history.Length; i++)
            {
                var previousPrice = Convert.ToInt32(history[i - 1].DeltaPercent);
                var currentPrice = Convert.ToInt32(history[i].DeltaPercent);

                if (!histogram.ContainsKey(previousPrice))
                    histogram[previousPrice] = new List<int>();
                histogram[previousPrice].Add(currentPrice);
            }

            var parameters = Enumerable.Range(1, 100)
                .Select(x => new ProbabilityParameters {Threshold = -x, Histogram = histogram});

            var optimal = _searcher.Maximum(parameters, fromDate, toDate);

            _parameters = (ProbabilityParameters) optimal;
        }

        public bool ShouldBuy(MarketData data)
        {
            var currentPrice = Convert.ToInt32(data.DeltaPercent);
            if (!_parameters.Histogram.ContainsKey(currentPrice))
                return false;
            
            var value = _parameters.Histogram[currentPrice].Average();
            return value > _parameters.Threshold;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ProbabilityStrategy)) return false;

            return Equals(obj as ProbabilityStrategy);
        }

        public bool Equals(ProbabilityStrategy other)
            => other._parameters.Threshold == _parameters.Threshold;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Threshold);
    }
}
