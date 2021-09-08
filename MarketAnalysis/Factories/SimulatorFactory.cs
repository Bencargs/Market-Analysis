using MarketAnalysis.Caching;
using MarketAnalysis.Simulation;
using System;
using System.Collections.Generic;
using MarketAnalysis.Services;
using MarketAnalysis.Staking;

namespace MarketAnalysis.Factories
{
    public class SimulatorFactory
    {
        private readonly IDictionary<Type, Func<ISimulator>> _typeLookup;

        public SimulatorFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache)
        {
            _typeLookup = new Dictionary<Type, Func<ISimulator>>
            {
                { typeof(TrainingSimulator), () => new TrainingSimulator(marketDataCache, simulationCache)},
                { typeof(BacktestingSimulator), () => new BacktestingSimulator(marketDataCache, simulationCache)}
            };
        }

        public ISimulator Create<T>()
            where T : ISimulator
        {
            var type = typeof(T);
            var constructor = _typeLookup[type];
            return constructor();
        }
    }
}
