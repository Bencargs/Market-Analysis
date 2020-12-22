using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class EntropyStrategy : IStrategy, IEquatable<EntropyStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IMarketDataCache _marketDataCache;
        private EntropyParameters _parameters;

        public IParameters Parameters
        {
            get => _parameters;
            private set => _parameters = (EntropyParameters)value;
        }
        public StrategyType StrategyType { get; } = StrategyType.Entropy;

        public EntropyStrategy(
            IMarketDataCache marketDataCache,
            ISearcher searcher,
            EntropyParameters parameters)
        {
            _searcher = searcher;
            _marketDataCache = marketDataCache;

            Parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(1, 30).SelectMany(w =>
            {
                return Enumerable.Range(1, 100).Select(e =>
                {
                    var threshold = (double)e / 10;
                    return new EntropyParameters { Window = w, Threshold = threshold };
                });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            Parameters = optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.GetLastSince(data.Date, _parameters.Window)
                .Select(x => x.Delta)
                .ToArray();

            var entropy = ShannonEntropy(batch);
            
            return entropy > _parameters.Threshold;
        }

        private static double ShannonEntropy<T>(T[] sequence)
        {
            var map = new Dictionary<T, int>();
            foreach (T c in sequence)
            {
                map.TryGetValue(c, out var currentCount);
                map[c] = currentCount + 1;
            }

            var entropies =
                from c in map.Keys
                let frequency = (double)map[c] / sequence.Length
                select -frequency * Math.Log(frequency) / Math.Log(2);

            return entropies.Sum();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(EntropyStrategy)) return false;

            return Equals(obj as EntropyStrategy);
        }

        public bool Equals(EntropyStrategy strategy)
            => strategy._parameters.Window == _parameters.Window &&
                Math.Abs(strategy._parameters.Threshold - _parameters.Threshold) < 0.00001;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Window, _parameters.Threshold);
    }
}
