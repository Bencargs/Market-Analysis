using MarketAnalysis.Caching;
using MarketAnalysis.Providers;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Factories
{
    public class StrategyFactory
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISimulationCache _simulationCache;
        private readonly OptimiserFactory _optimiserFactory;

        public StrategyFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider)
        {
            _marketDataCache = marketDataCache;
            _simulationCache = simulationCache;
            _optimiserFactory = new OptimiserFactory(_marketDataCache, _simulationCache, investorProvider, this);
        }

        public IStrategy Create(IParameters parameters)
        {
            return parameters switch
            {
                LinearRegressionParameters p => Create(p),
                RelativeStrengthParameters p => Create(p),
                DeltaParameters p => Create(p),
                VolumeParameters p => Create(p),
                GradientParameters p => Create(p),
                EntropyParameters p => Create(p),
                StaticDatesParameters p => Create(p),
                MovingAverageParameters p => Create(p),
                HolidayEffectParameters p => Create(p),
                WeightedParameters p => Create(p),
                OptimalStoppingParameters p => Create(p),
                ProbabilityParameters p => Create(p),
                SpreadParameters p => Create(p),
                _ => throw new NotImplementedException(),
            };
        }

        private IStrategy Create(LinearRegressionParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new LinearRegressionStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(RelativeStrengthParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new RelativeStrengthStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(DeltaParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new DeltaStrategy(
                optimiser,
                parameters);
        }

        private IStrategy Create(VolumeParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new VolumeStrategy(
                optimiser,
                parameters);
        }

        private IStrategy Create(GradientParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new GradientStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(EntropyParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new EntropyStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(MovingAverageParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new MovingAverageStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }
        
        private IStrategy Create(WeightedParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new WeightedStrategy(
                _simulationCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(OptimalStoppingParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new OptimalStoppingStrategy(
                optimiser,
                parameters);
        }
        
        private IStrategy Create(ProbabilityParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new ProbabilityStrategy(
                _marketDataCache,
                optimiser,
                parameters);
        }

        private IStrategy Create(SpreadParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new SpreadStrategy(
                optimiser,
                parameters);
        }

        private static IStrategy Create(StaticDatesParameters parameters)
            => new StaticDatesStrategy(parameters);

        private static IStrategy Create(HolidayEffectParameters parameters)
            => new HolidayEffectStrategy(parameters);
    }
}
