using MarketAnalysis.Caching;
using MarketAnalysis.Providers;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using System;
using MarketAnalysis.Services;
using MarketAnalysis.Staking;

namespace MarketAnalysis.Factories
{
    public class StrategyFactory
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISimulationCache _simulationCache;
        private readonly OptimiserFactory _optimiserFactory;
        private readonly RatingService _ratingService;

        public StrategyFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider,
            RatingService ratingService)
        {
            _marketDataCache = marketDataCache;
            _simulationCache = simulationCache;
            _ratingService = ratingService;
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
                ClusteringParameters p => Create(p),
                MultipleParameters p => Create(p),
                _ => throw new NotImplementedException(),
            };
        }

        private IStrategy Create(LinearRegressionParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new LinearRegressionStrategy(
                _marketDataCache,
                optimiser,
                new BasicKellyStaking(_marketDataCache),
                parameters);
        }

        private IStrategy Create(RelativeStrengthParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new RelativeStrengthStrategy(
                _marketDataCache,
                new BasicKellyStaking(_marketDataCache),
                optimiser,
                parameters);
        }

        private IStrategy Create(DeltaParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new DeltaStrategy(
                optimiser,
                new DollarValueAveraging(_marketDataCache),
                parameters);
        }

        private IStrategy Create(VolumeParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new VolumeStrategy(
                optimiser,
                new DollarValueAveraging(_marketDataCache),
                parameters);
        }

        private IStrategy Create(GradientParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new GradientStrategy(
                _marketDataCache,
                new BasicKellyStaking(_marketDataCache),
                optimiser,
                parameters);
        }

        private IStrategy Create(EntropyParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new EntropyStrategy(
                _marketDataCache,
                optimiser,
                new DollarCostAveraging(),
                parameters);
        }

        private IStrategy Create(MovingAverageParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new MovingAverageStrategy(
                _marketDataCache,
                new DollarCostAveraging(),
                optimiser,
                parameters);
        }
        
        private IStrategy Create(WeightedParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new WeightedStrategy(
                _simulationCache,
                new DollarCostAveraging(),
                optimiser,
                parameters);
        }

        private IStrategy Create(MultipleParameters parameters)
        {
            return new MultipleStrategy(
                new DollarCostAveraging(),
                parameters);
        }

        private IStrategy Create(OptimalStoppingParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new OptimalStoppingStrategy(
                optimiser,
                new DollarCostAveraging(),
                parameters);
        }
        
        private IStrategy Create(ProbabilityParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new ProbabilityStrategy(
                _marketDataCache,
                optimiser,
                new DollarValueAveraging(_marketDataCache),
                parameters);
        }

        private IStrategy Create(SpreadParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new SpreadStrategy(
                optimiser,
                new DollarValueAveraging(_marketDataCache),
                parameters);
        }

        private IStrategy Create(ClusteringParameters parameters)
        {
            var optimiser = _optimiserFactory.Create<LinearSearch>();

            return new ClusteringStrategy(
                optimiser,
                _marketDataCache,
                _ratingService,
                new DollarValueAveraging(_marketDataCache),
                parameters);
        }

        private static IStrategy Create(StaticDatesParameters parameters)
            => new StaticDatesStrategy(parameters);

        private IStrategy Create(HolidayEffectParameters parameters)
            => new HolidayEffectStrategy(parameters, new DollarValueAveraging(_marketDataCache));
    }
}
