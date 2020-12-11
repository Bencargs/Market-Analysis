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
        private readonly MarketDataCache _dataCache;
        private readonly SimulationCache _simulationCache;
        private readonly InvestorProvider _investorProvider;

        public LinearSearch(
            MarketDataCache dataCache,
            SimulationCache simulationCache,
            InvestorProvider investorProvider)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;
            _investorProvider = investorProvider;
        }

        public T Maximum<T>(
            IEnumerable<T> strategies, 
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
            //ClearCache(potentials, optimal);

            return optimal;
        }

        //private void ClearCache(IEnumerable<IStrategy> potentials, IStrategy optimal)
        //{
        //    _simulator.RemoveCache(potentials.Except(new[] { optimal }));
        //}
    }
}
