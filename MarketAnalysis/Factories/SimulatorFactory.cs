using MarketAnalysis.Caching;
using MarketAnalysis.Simulation;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Factories
{
    public class SimulatorFactory
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISimulationCache _simulationCache;
        private readonly IDictionary<Type, Func<ISimulator>> _typeLookup;

        public SimulatorFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache)
        {
            _marketDataCache = marketDataCache;
            _simulationCache = simulationCache;
            _typeLookup = new Dictionary<Type, Func<ISimulator>>
            {
                { typeof(TrainingSimulator), () => new TrainingSimulator(_marketDataCache, _simulationCache) },
                { typeof(BacktestingSimulator), () => new BacktestingSimulator(_marketDataCache, _simulationCache) }
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
