using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly MarketDataCache _marketDataCache;
        private readonly IResultsProvider _resultsProvider;
        private readonly StrategyProvider _strategyProvider;
        private readonly InvestorProvider _investorProvider;
        private readonly SimulatorFactory _simulatorFactory;
        private readonly MarketDataProvider _marketDataProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            MarketDataCache marketDataCache,
            IResultsProvider resultsProvider,
            StrategyProvider strategyProvider,
            InvestorProvider investorProvider,
            SimulatorFactory simulatorFactory,
            MarketDataProvider marketDataProvider,
            ICommunicationService communicationService)
        {
            _simulatorFactory = simulatorFactory;
            _marketDataCache = marketDataCache;
            _resultsProvider = resultsProvider;
            _strategyProvider = strategyProvider;
            _investorProvider = investorProvider;
            _marketDataProvider = marketDataProvider;
            _communicationService = communicationService;
        }

        public async Task Execute()
        {
            (var strategies, var data) = await Initialise();

            var results = Simulate(strategies);

            await _communicationService.SendCommunication(results);

            await SaveResults(results, data);
        }

        private async Task<(IEnumerable<IStrategy>, IEnumerable<MarketData>)> Initialise()
        {
            var dataTask = _marketDataProvider.GetPriceData();
            var strategies = _strategyProvider.GetStrategies();
            _investorProvider.Initialise();

            var data = await dataTask;
            _marketDataCache.Initialise(data);

            return (strategies, data);
        }

        private IResultsProvider Simulate(IEnumerable<IStrategy> strategies)
        {
            foreach (var investor in _investorProvider)
            {
                _resultsProvider.Initialise();
                var histories = new ConcurrentDictionary<IStrategy, SimulationState[]>();
                using var progress = ProgressBarProvider.Create(0, "Evaluating...");
                //Parallel.ForEach(strategies, strategy =>
                foreach (var strategy in strategies)
                {
                    var description = strategy.StrategyType.GetDescription();
                    Log.Information($"Simulating strategy: {description}");

                    var simulator = _simulatorFactory.Create<BacktestingSimulator>();
                    var result = simulator.Evaluate(strategy, _investorProvider.Current, progress: progress);
                    histories[strategy] = result.ToArray();
                }
                //);
                _resultsProvider.AddResults(investor, histories);
            }

            return _resultsProvider;
        }

        private static async Task SaveResults(IResultsProvider resultsProvider, IEnumerable<MarketData> data)
        {
            var saveSimulationsTask = resultsProvider.SaveSimulationResults();

            var saveDataTask = resultsProvider.SaveData(data);

            await Task.WhenAll(saveSimulationsTask, saveDataTask);
        }
    }
}
