using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Serilog;
using ShellProgressBar;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly ISimulator _simulator;
        private readonly IResultsProvider _resultsProvider;
        private readonly StrategyProvider _strategyProvider;
        private readonly MarketDataProvider _marketDataProvider;
        private readonly ProgressBarProvider _progressBarProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            ISimulator simulator,
            IResultsProvider resultsProvider,
            StrategyProvider strategyProvider,
            MarketDataProvider marketDataProvider,
            ProgressBarProvider progressBarProvider,
            ICommunicationService communicationService)
        {
            _simulator = simulator;
            _resultsProvider = resultsProvider;
            _strategyProvider = strategyProvider;
            _marketDataProvider = marketDataProvider;
            _progressBarProvider = progressBarProvider;
            _communicationService = communicationService;
        }

        public async Task Execute()
        {
            (var data, var strategies) = await GetSimulationData();

            var results = await Simulate(data, strategies);

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

        private async Task<IResultsProvider> Simulate(IEnumerable<MarketData> data, IEnumerable<IStrategy> strategies)
        {
            void SimulateStrategy(IStrategy strategy, Dictionary<IStrategy, SimulationState[]> histories, ProgressBar progress)
            {
                Log.Information($"Evaluating strategy: {strategy.StrategyType.GetDescription()}");
                var result = _simulator.Evaluate(strategy, progress: progress);
                histories[strategy] = result.ToArray();
            }

            using (var cache = MarketDataCache.Instance)
            {
                cache.Initialise(data);
                _resultsProvider.Initialise();

                var (aggregateStrategies, parralisableStrategies) = strategies.Split(x => x is IAggregateStrategy);
                var histories = new Dictionary<IStrategy, SimulationState[]>();
                using (var progress = _progressBarProvider.Create(0, "Evaluating"))
                {
                    await Task.WhenAll(parralisableStrategies.Select(s =>
                    {
                        return Task.Run(() => SimulateStrategy(s, histories, progress));
                    }).ToArray());
                    foreach (var strategy in aggregateStrategies)
                    {
                        SimulateStrategy(strategy, histories, progress);
                    }
                }
                _resultsProvider.AddResults(histories);
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
