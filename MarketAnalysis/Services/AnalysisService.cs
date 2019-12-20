using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly ISimulator _simulator;
        private readonly IResultsProvider _resultsProvider;
        private readonly StrategyProvider _strategyProvider;
        private readonly MarketDataProvider _marketDataProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            ISimulator simulator,
            IResultsProvider resultsProvider,
            StrategyProvider strategyProvider,
            MarketDataProvider marketDataProvider,
            ICommunicationService communicationService)
        {
            _simulator = simulator;
            _resultsProvider = resultsProvider;
            _strategyProvider = strategyProvider;
            _marketDataProvider = marketDataProvider;
            _communicationService = communicationService;
        }

        public async Task Execute()
        {
            var data = await _marketDataProvider.GetPriceData();
            var strategies = await _strategyProvider.GetStrategies();

            var results = Simulate(data, strategies);

            if (results.ShouldBuy())
                await _communicationService.SendCommunication(results);

            await results.SaveSimulationResults();
            await results.SaveData(data);
        }

        private IResultsProvider Simulate(IEnumerable<MarketData> data, IEnumerable<IStrategy> strategies)
        {
            using (var cache = MarketDataCache.Instance)
            {
                cache.Initialise(data);
                _resultsProvider.Initialise();

                foreach (var s in strategies)
                {
                    Log.Information($"Evaluating strategy: {s.GetType()}");
                    var history = _simulator.Evaluate(s);
                    _resultsProvider.AddResults(s, history);
                }
            }
            return _resultsProvider;
        }
    }
}
