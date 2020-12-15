using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy
    {
        private readonly StrategyFactory _strategyFactory;
        private readonly ISearcher _searcher;
        private DeltaParameters _parameters;

        public IParameters Parameters
        {
            get => _parameters;
            private set => _parameters = (DeltaParameters)value;
        }
        public StrategyType StrategyType { get; } = StrategyType.Delta;

        public DeltaStrategy(
            StrategyFactory strategyFactory,
            ISearcher searcher,
            DeltaParameters parameters)
        {
            _strategyFactory = strategyFactory;
            _searcher = searcher;

            Parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(1, 100).Select(x =>
            {
                var threshold = (decimal)x / 1000;
                return _strategyFactory.Create(new DeltaParameters { Threshold = threshold });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            Parameters = optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            return data.Delta < _parameters.Threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DeltaStrategy strategy))
                return false;

            return strategy._parameters.Threshold == _parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return _parameters.Threshold.GetHashCode();
        }
    }
}
