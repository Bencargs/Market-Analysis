using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Simulation;
using MathNet.Numerics;
using ShellProgressBar;

namespace MarketAnalysis.Strategy
{
    public class GradientStrategy : OptimisableStrategy
    {
        private int _window;
        private decimal _threshold;
        private readonly MarketDataCache _marketDataCache;
        public override StrategyType StrategyType { get; } = StrategyType.Gradient;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(256);

        public GradientStrategy(MarketDataCache marketDataCache)
            : this (marketDataCache, 0, 0)
        { }

        public GradientStrategy(
            MarketDataCache marketDataCache,
            int window, 
            decimal threshold, 
            bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _window = window;
            _threshold = threshold;
            _marketDataCache = marketDataCache;
        }

        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
        {
            var potentials = Enumerable.Range(1, 10).SelectMany(x =>
            {
                return Enumerable.Range(20, 20).Select(window =>
                {
                    var threshold = -((decimal)x / 100);
                    return new GradientStrategy(_marketDataCache, window, threshold, false);
                });
            });

            var searcher = new LinearSearch(simulator, potentials, progress);
            return searcher.Maximum(LatestDate);
        }

        protected override void SetParameters(IStrategy strategy)
        {
            var optimal = (GradientStrategy)strategy;
            _window = optimal._window;
            _threshold = optimal._threshold;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.TakeUntil(LatestDate).ToList().Last(_window);
            if (batch.Length < 2)
                return false;

            var xData = batch.Select(x => (double)x.Price).ToArray();
            var yData = Enumerable.Range(0, batch.Length).Select(x => (double)x).ToArray();
            var parameters = Fit.Line(xData, yData);

            return parameters.Item2 < (double)_threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GradientStrategy strategy))
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
