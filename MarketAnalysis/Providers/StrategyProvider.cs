using MarketAnalysis.Factories;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using Serilog;
using System.Collections.Generic;

namespace MarketAnalysis.Providers
{
    public class StrategyProvider
    {
        private readonly StrategyFactory _strategyFactory;

        public StrategyProvider(StrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public IEnumerable<IStrategy> GetStrategies()
        {
            var subStrategies = new IStrategy[]
            {
                _strategyFactory.Create(new RelativeStrengthParameters()),
                _strategyFactory.Create(new DeltaParameters()),
                _strategyFactory.Create(new GradientParameters()),
                _strategyFactory.Create(new LinearRegressionParameters()),
                _strategyFactory.Create(new VolumeParameters()),

            };
            //var strategies = subStrategies.Concat(new[] { _strategyFactory.Create(new WeightedStrategyParameters()) });
            var strategies = subStrategies;

            Log.Information($"Evaluating against strategies: {string.Join<IStrategy>(", ", strategies)}");

            return strategies;
        }
    }
}
