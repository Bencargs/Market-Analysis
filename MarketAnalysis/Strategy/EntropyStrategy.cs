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
    public class EntropyStrategy : OptimisableStrategy
    {
        private double _threshold;
        private int _window;
        private readonly MarketDataCache _marketDataCache;
        
        public override StrategyType StrategyType { get; } = StrategyType.Entropy;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(512);

        public EntropyStrategy(MarketDataCache marketDataCache)
            : this (marketDataCache, 0, 0)
        { }

        public EntropyStrategy(
            MarketDataCache marketDataCache, 
            int window, 
            double threshold, 
            bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _window = window;
            _threshold = threshold;
            _marketDataCache = marketDataCache;
        }

        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
        {
            var potentials = Enumerable.Range(1, 30).SelectMany(w =>
            {
                return Enumerable.Range(1, 100).Select(e =>
                {
                    var threshold = (double)e / 10;
                    return new EntropyStrategy(_marketDataCache, w, threshold, false);
                });
            });

            var searcher = new LinearSearch(simulator, potentials, progress);
            return searcher.Maximum(LatestDate);
        }

        protected override void SetParameters(IStrategy strategy)
        {
            var optimal = (EntropyStrategy)strategy;
            _window = optimal._window;
            _threshold = optimal._threshold;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.GetLastSince(data.Date, _window)
                .Select(x => x.Delta)
                .ToArray();

            var entropy = ShannonEntropy(batch);
            if (entropy > _threshold)
                return true;
            return false;
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
            if (!(obj is EntropyStrategy strategy))
                return false;

            return strategy._window == _window &&
                   strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _window.GetHashCode() ^ _threshold.GetHashCode();
        }
    }
}
