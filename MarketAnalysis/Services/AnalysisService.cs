using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
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
        private readonly InvestorProvider _investorProvider;
        private readonly MarketDataProvider _marketDataProvider;
        private readonly ProgressBarProvider _progressBarProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            ISimulator simulator,
            IResultsProvider resultsProvider,
            StrategyProvider strategyProvider,
            InvestorProvider investorProvider,
            MarketDataProvider marketDataProvider,
            ProgressBarProvider progressBarProvider,
            ICommunicationService communicationService)
        {
            _simulator = simulator;
            _resultsProvider = resultsProvider;
            _strategyProvider = strategyProvider;
            _investorProvider = investorProvider;
            _marketDataProvider = marketDataProvider;
            _progressBarProvider = progressBarProvider;
            _communicationService = communicationService;
        }

        public async Task Execute()
        {
            (var strategies, var data) = await Initialise();

            var results = await Simulate(strategies);

            await _communicationService.SendCommunication(results);

            await SaveResults(results, data);
        }

        private async Task<(IEnumerable<IStrategy>, IEnumerable<MarketData>)> Initialise()
        {
            var strategiesTask = _strategyProvider.GetStrategies();
            var dataTask = _marketDataProvider.GetPriceData();
            var investorTask = _investorProvider.Initialise();
            
            await Task.WhenAll(strategiesTask, dataTask, investorTask);
            var strategies = strategiesTask.Result;
            var data = dataTask.Result;
            MarketDataCache.Instance.Initialise(data);

            return (strategies, data);
        }

        private async Task<IResultsProvider> Simulate(IEnumerable<IStrategy> strategies)
        {
            void SimulateStrategy(IStrategy strategy, Dictionary<IStrategy, SimulationState[]> histories, ProgressBar progress)
            {
                Log.Information($"Evaluating strategy: {strategy.StrategyType.GetDescription()}");
                var result = _simulator.Evaluate(strategy, progress: progress);
                histories[strategy] = result.ToArray();
            }

            var histories = new Dictionary<IStrategy, SimulationState[]>();
            var (aggregateStrategies, parralisableStrategies) = strategies.Split(x => x is IAggregateStrategy);

            using var progress = _progressBarProvider.Create(0, "Evaluating");
            foreach (var investor in _investorProvider)// these two lines are super fragile - replace with observer pattern
            {
                _resultsProvider.Initialise();
                await Task.WhenAll(parralisableStrategies.Select(s =>
                {
                    return Task.Run(() => SimulateStrategy(s, histories, progress));
                }).ToArray());
                foreach (var strategy in aggregateStrategies)
                {
                    SimulateStrategy(strategy, histories, progress);
                }
                _resultsProvider.AddResults(investor, histories);
            }

            return await Task.FromResult(_resultsProvider);
        }

        private async Task SaveResults(IResultsProvider resultsProvider, IEnumerable<MarketData> data)
        {
            var saveSimulationsTask = resultsProvider.SaveSimulationResults();

            var saveDataTask = resultsProvider.SaveData(data);

            await Task.WhenAll(new Task[] { saveSimulationsTask, saveDataTask });
        }
    }
}
