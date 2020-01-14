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
            var data = _marketDataProvider.GetPriceData();
            var strategies = _strategyProvider.GetStrategies();

            var results = await Simulate(data, strategies);

            var sendCommunicationTask = results.ShouldBuy()
                ? _communicationService.SendCommunication(results)
                : Task.CompletedTask;
            var saveSimulationsTask = results.SaveSimulationResults();
            var saveDataTask = results.SaveData(data);

            await sendCommunicationTask;
            await saveSimulationsTask;
            await saveDataTask;
        }

        private async Task<IResultsProvider> Simulate(Task<IEnumerable<MarketData>> data, Task<IEnumerable<IStrategy>> strategies)
        {
            using (var cache = MarketDataCache.Instance)
            {
                cache.Initialise(await data);
                _resultsProvider.Initialise();

                foreach (var s in await strategies)
                {
                    Log.Information($"Evaluating strategy: {s.StrategyType.GetDescription()}");
                    var history = _simulator.Evaluate(s);
                    _resultsProvider.AddResults(s, history);
                }
            }
            return _resultsProvider;
        }
    }
}
