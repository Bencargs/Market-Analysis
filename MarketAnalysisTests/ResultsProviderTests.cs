using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Services;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using MarketAnalysisTests.ApprovalTests;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarketAnalysisTests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ChartTests")]
    public class ResultsProviderTests : TestHarness
    {
        private IResultsProvider _target;

        [SetUp]
        public void Setup()
        {
            ConfigurationManager.AppSettings["BacktestingDate"] = "2010-07-01";
            ConfigurationManager.AppSettings["CacheSize"] = "2000";
            ConfigurationManager.AppSettings["DataPath"] = "MarketData.csv";
            ConfigurationManager.AppSettings["RelativePath"] = @"../../../";

            var marketData = CreateMarketData();
            var marketDataCache = CreateMarketDataCache(marketData);

            var investor = new Investor { DailyFunds = 10, OrderDelayDays = 3 };
            var investorProvider = CreateInvestorProvider();

            var simulationCache = new SimulationCache();

            var simulationFactory = new SimulatorFactory(marketDataCache, simulationCache);
            var ratingService = new RatingService(
                marketDataCache,
                simulationFactory,
                investorProvider);

            
            var strategyFactory = CreateStrategyFactory(
                marketDataCache, 
                simulationCache,
                investorProvider,
                ratingService);

            var marketDataRepository = new Mock<IRepository<MarketData>>();
            var simulationResultsRepository = new Mock<IRepository<SimulationResult>>();

            _target = new ResultsProvider(
                marketDataCache,
                marketDataRepository.Object,
                ratingService,
                simulationResultsRepository.Object);

            _target.Initialise();

            var strategy = strategyFactory.Create(new HolidayEffectParameters());
            var stateJson = File.ReadAllText(@"HolidayEffectSimulationState.json");
            var simulationState = JsonConvert.DeserializeObject<SimulationState[]>(stateJson);
            var resultsToAdd = new ConcurrentDictionary<IStrategy, SimulationState[]>();
            resultsToAdd.TryAdd(strategy, simulationState);

            _target.AddResults(investor, resultsToAdd);
        }

        [Test]
        public async Task PerformanceChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "ChartTests", "PerformanceChart.png");
            
            await _target.SaveChart(ResultsChart.Performance, path);
            
            Approvals.Verify(new FileInfo(path));
        }

        [Test]
        public async Task RelativeChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "ChartTests", "RelativeChart.png");

            await _target.SaveChart(ResultsChart.Relative, path);

            Approvals.Verify(new FileInfo(path));
        }

        [Test]
        public async Task SignalChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "ChartTests", "SignalChart.png");

            await _target.SaveChart(ResultsChart.Signal, path);

            Approvals.Verify(new FileInfo(path));
        }
    }
}
