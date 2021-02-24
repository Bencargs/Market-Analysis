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
using ShellProgressBar;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly IResultsProvider _resultsProvider;
        private readonly StrategyProvider _strategyProvider;
        private readonly IInvestorProvider _investorProvider;
        private readonly SimulatorFactory _simulatorFactory;
        private readonly MarketDataProvider _marketDataProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            IMarketDataCache marketDataCache,
            IResultsProvider resultsProvider,
            StrategyProvider strategyProvider,
            IInvestorProvider investorProvider,
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
            var (strategies, data) = await Initialise();

            var results = Simulate(strategies);

            await _communicationService.SendCommunication(results);

            await SaveResults(results, data);
        }

        private async Task<(IEnumerable<IStrategy>, IEnumerable<MarketData>)> Initialise()
        {
            var dataTask = _marketDataProvider.GetPriceData();
            var strategies = _strategyProvider.GetStrategies();
            _investorProvider.Initialise();

            var data = (await dataTask).ToArray();
            _marketDataCache.Initialise(data);

            return (strategies, data);
        }

        private IResultsProvider Simulate(IEnumerable<IStrategy> strategies)
        {
            SimulationState[] SimulateStrategy(IStrategy strategy, ProgressBar progress)
            {
                var description = strategy.StrategyType.GetDescription();
                Log.Information($"Simulating strategy: {description}");

                var simulator = _simulatorFactory.Create<BacktestingSimulator>();
                var result = simulator.Evaluate(strategy, _investorProvider.Current, progress: progress);
                return result.ToArray();
            }

            foreach (var investor in _investorProvider)
            {
                _resultsProvider.Initialise();
                var histories = new ConcurrentDictionary<IStrategy, SimulationState[]>();
                using var progress = ProgressBarProvider.Create(0, "Evaluating...");
                var (sequential, parallelisable) = strategies.Split(x => x is IAggregateStrategy);
                Parallel.ForEach(parallelisable, strategy =>
                {
                    histories[strategy] = SimulateStrategy(strategy, progress);
                });
                foreach (var strategy in sequential)
                {
                    histories[strategy] = SimulateStrategy(strategy, progress);
                }
                _resultsProvider.AddResults(investor, histories);
            }

            return _resultsProvider;
        }

        private static async Task SaveResults(IResultsProvider resultsProvider, IEnumerable<MarketData> data)
        {
            var saveSimulationsTask = resultsProvider.SaveSimulationResults();

            var saveDataTask = resultsProvider.SaveData(data);//sus - just use the cache?

            var performanceChartTask = resultsProvider.SaveChart(ResultsChart.Performance, @"C:\temp\performance.png");
            var relativeChartTask = resultsProvider.SaveChart(ResultsChart.Relative, @"C:\temp\relative.png");
            var signalChartTask = resultsProvider.SaveChart(ResultsChart.Signal, @"C:\temp\buys.png");

            await Task.WhenAll(saveSimulationsTask, saveDataTask, performanceChartTask, relativeChartTask, signalChartTask);
        }
    }
}
