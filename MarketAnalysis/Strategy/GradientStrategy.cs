using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using MathNet.Numerics;

namespace MarketAnalysis.Strategy
{
    public class GradientStrategy : IStrategy
    {
        private readonly StrategyFactory _strategyFactory;
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISearcher _searcher;
        private GradientParameters _parameters;

        public IParameters Parameters
        {
            get => _parameters;
            private set => _parameters = (GradientParameters)value;
        }
        public StrategyType StrategyType { get; } = StrategyType.Gradient;

        public GradientStrategy(
            StrategyFactory strategyFactory,
            IMarketDataCache marketDataCache,
            ISearcher searcher,
            GradientParameters parameters)
        {
            _strategyFactory = strategyFactory;
            _marketDataCache = marketDataCache;
            _searcher = searcher;
            Parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(1, 10).SelectMany(x =>
            {
                return Enumerable.Range(20, 20).Select(window =>
                {
                    var threshold = -((decimal)x / 100);
                    return _strategyFactory.Create(new GradientParameters { Threshold = threshold, Window = window });
                });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            Parameters = optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.TakeUntil(data.Date).ToList().Last(_parameters.Window);
            if (batch.Length < 2)
                return false;

            var xData = batch.Select(x => (double)x.Price).ToArray();
            var yData = Enumerable.Range(0, batch.Length).Select(x => (double)x).ToArray();
            var (intercept, gradient) = Fit.Line(xData, yData);

            return gradient < (double)_parameters.Threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GradientStrategy strategy))
                return false;

            return strategy._parameters.Window == _parameters.Window &&
                   strategy._parameters.Threshold == _parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return _parameters.Window.GetHashCode() ^ _parameters.Threshold.GetHashCode();
        }
    }
}
