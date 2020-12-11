using MarketAnalysis.Caching;
using MarketAnalysis.Providers;
using MarketAnalysis.Search;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Factories
{
    public class OptimiserFactory
    {
        private readonly MarketDataCache _marketDataCache;
        private readonly SimulationCache _simulationCache;
        private readonly InvestorProvider _investorProider;
        private readonly Dictionary<Type, Func<ISearcher>> _typeLookup;

        public OptimiserFactory(
            MarketDataCache marketDataCache,
            SimulationCache simulationCache,
            InvestorProvider investorProider)
        {
            _marketDataCache = marketDataCache;
            _simulationCache = simulationCache;
            _investorProider = investorProider;
            _typeLookup = new Dictionary<Type, Func<ISearcher>>
            {
                { typeof(LinearSearch), () => new LinearSearch(_marketDataCache, _simulationCache, _investorProider) },
            };
        }

        public ISearcher Create<T>()
            where T : ISearcher
        {
            var type = typeof(T);
            var constructor = _typeLookup[type];
            return constructor();
        }
    }
}
