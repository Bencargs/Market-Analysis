using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarketAnalysis.Search
{
    public class LinearSearch : ISearcher
    {
        private readonly IMarketDataCache _dataCache;
        private readonly ISimulationCache _simulationCache;
        private readonly IInvestorProvider _investorProvider;
        private readonly StrategyFactory _strategyFactory;

        public LinearSearch(
            IMarketDataCache dataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider,
            StrategyFactory strategyFactory)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;
            _investorProvider = investorProvider;
            _strategyFactory = strategyFactory;
        }

        public IParameters Maximum(
            IEnumerable<IParameters> parameters,
            DateTime fromDate,
            DateTime endDate)
        {
            var potentials = parameters.Select((param, index) =>
            {
                var strategy = _strategyFactory.Create(param);
                var investor = _investorProvider.Current;
                var simulator = new TrainingSimulator(_dataCache, _simulationCache);
                var result = simulator.Evaluate(strategy, investor, endDate).Last();
                return (result.Worth, result.BuyCount, strategy, index);
            })
            .AsParallel()
            .OrderByDescending(x => x.Worth)
            .ThenBy(x => x.index) // This is used as a tie breaker due to parallelism
            .Select(x => x.strategy)
            .ToArray();

            var optimal = potentials.First();
            var toRemove = potentials.Except(new[] { optimal }.AsParallel());
            _simulationCache.Remove(fromDate, endDate, toRemove);

            return optimal.Parameters;
        }
    }
}
