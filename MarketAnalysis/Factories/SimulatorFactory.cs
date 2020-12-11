using MarketAnalysis.Caching;
using MarketAnalysis.Simulation;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Factories
{
    public class SimulatorFactory
    {
        private readonly MarketDataCache _marketDataCache;
        private readonly SimulationCache _simulationCache;
        private readonly Dictionary<Type, Func<ISimulator>> _typeLookup;

        public SimulatorFactory(
            MarketDataCache marketDataCache,
            SimulationCache simulationCache)
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
