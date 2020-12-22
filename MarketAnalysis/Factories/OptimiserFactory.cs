using MarketAnalysis.Caching;
using MarketAnalysis.Providers;
using MarketAnalysis.Search;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Factories
{
    public class OptimiserFactory
    {
        private readonly Dictionary<Type, Func<ISearcher>> _typeLookup;

        public OptimiserFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider,
            StrategyFactory strategyFactory)
        {
            _typeLookup = new Dictionary<Type, Func<ISearcher>>
            {
                { typeof(LinearSearch), () => new LinearSearch(marketDataCache, simulationCache, investorProvider, strategyFactory) },
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
