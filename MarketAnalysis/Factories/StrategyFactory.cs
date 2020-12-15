using MarketAnalysis.Caching;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Factories
{
    public class StrategyFactory
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly OptimiserFactory _optimiserFactory;

        public StrategyFactory(
            IMarketDataCache marketDataCache,
            OptimiserFactory optimiserFactory)
        {
            _marketDataCache = marketDataCache;
            _optimiserFactory = optimiserFactory;
        }

        public IStrategy Create(LinearRegressionParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new LinearRegressionStrategy(
                this,
                _marketDataCache,
                optimiser,
                parameters);
        }

        public IStrategy Create(RelativeStrengthParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new RelativeStrengthStrategy(
                this,
                _marketDataCache,
                optimiser,
                parameters);
        }

        public IStrategy Create(DeltaParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new DeltaStrategy(
                this,
                optimiser,
                parameters);
        }

        public IStrategy Create(VolumeParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new VolumeStrategy(
                this,
                optimiser,
                parameters);
        }

        public IStrategy Create(GradientParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new GradientStrategy(
                this,
                _marketDataCache,
                optimiser,
                parameters);
        }

        public IStrategy Create(StaticDatesParameters parameters)
            => new StaticDatesStrategy(parameters);
    }
}
