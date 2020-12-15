using MarketAnalysis;
using MarketAnalysis.Caching;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Search
{
    public class LinearSearch : ISearcher
    {
        private readonly IMarketDataCache _dataCache;
        private readonly ISimulationCache _simulationCache;
        private readonly IInvestorProvider _investorProvider;

        public LinearSearch(
            IMarketDataCache dataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;
            _investorProvider = investorProvider;
        }

        public T Maximum<T>(
            IEnumerable<T> strategies, 
            DateTime fromDate,
            DateTime endDate)
            where T : IStrategy
        {
            var potentials = strategies.Select(strat =>
            {
                var investor = _investorProvider.Current;
                var simulator = new TrainingSimulator(_dataCache, _simulationCache);
                var result = simulator.Evaluate(strat, investor, endDate).Last();
                return (result.Worth, result.BuyCount, strat);
            })
            .AsParallel()
            .OrderByDescending(x => x.Worth)
            .ThenBy(x => x.BuyCount)
            .Select(x => x.strat);
            
            var optimal = potentials.First();
            var toRemove = strategies.Except(new[] { optimal }).Cast<IStrategy>();
            _simulationCache.Remove(fromDate, endDate, toRemove);

            return optimal;
        }
    }
}
