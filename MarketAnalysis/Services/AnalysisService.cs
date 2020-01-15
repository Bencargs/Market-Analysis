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
            (var data, var strategies) = await GetSimulationData();

            var results = Simulate(data, strategies);

            if (results.ShouldBuy())
                await _communicationService.SendCommunication(results);

            await SaveResults(data, results);
        }

        private async Task<(IEnumerable<MarketData>, IEnumerable<IStrategy>)> GetSimulationData()
        {
            var dataTask = _marketDataProvider.GetPriceData();
            var strategiesTask = _strategyProvider.GetStrategies();

            return (await dataTask, await strategiesTask);
        }

        private IResultsProvider Simulate(IEnumerable<MarketData> data, IEnumerable<IStrategy> strategies)
        {
            using (var cache = MarketDataCache.Instance)
            {
                cache.Initialise(data);
                _resultsProvider.Initialise();

                foreach (var s in strategies)
                {
                    Log.Information($"Evaluating strategy: {s.StrategyType.GetDescription()}");
                    var history = _simulator.Evaluate(s);
                    _resultsProvider.AddResults(s, history);
                }
            }
            return _resultsProvider;
        }

        private async Task SaveResults(IEnumerable<MarketData> data, IResultsProvider resultsProvider)
        {
            var saveSimulationsTask = resultsProvider.SaveSimulationResults();
            var saveDataTask = resultsProvider.SaveData(data);

            await Task.WhenAll(new Task[] { saveSimulationsTask, saveDataTask });
        }
    }
}
