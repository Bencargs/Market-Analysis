﻿using MarketAnalysis.Factories;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Providers
{
    public class StrategyProvider
    {
        private readonly StrategyFactory _strategyFactory;

        public StrategyProvider(StrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public IStrategy[] GetStrategies()
        {
            var subStrategies = new[]
            {
                _strategyFactory.Create(new RelativeStrengthParameters()),
                _strategyFactory.Create(new DeltaParameters()),
                _strategyFactory.Create(new GradientParameters()),
                _strategyFactory.Create(new LinearRegressionParameters()),
                _strategyFactory.Create(new VolumeParameters()),
                _strategyFactory.Create(new HolidayEffectParameters()),
                _strategyFactory.Create(new MovingAverageParameters()),
                _strategyFactory.Create(new OptimalStoppingParameters()),
                _strategyFactory.Create(new ProbabilityParameters()),
                //_strategyFactory.Create(new EntropyParameters()),
                _strategyFactory.Create(new ClusteringParameters())
                //_strategyFactory.Create(new WeightedParameters())
            };
            //var strategies = subStrategies.Concat(new[]
            //{
            //    _strategyFactory.Create(new WeightedParameters {Weights = subStrategies.ToDictionary(x => x, v => 0d)})
            //});
            var strategies = subStrategies;

            Log.Information($"Evaluating against strategies: {string.Join<IStrategy>(", ", strategies)}");

            return strategies.ToArray();
        }
    }
}
