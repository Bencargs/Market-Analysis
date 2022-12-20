using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
            var marketStatisticService = new MarketStatisticsProvider(marketDataCache);

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
                marketStatisticService,
                simulationResultsRepository.Object);

            _target.Initialise();

            var strategy = strategyFactory.Create(new RelativeStrengthParameters());
            var stateJson = File.ReadAllText(@"HolidayEffectSimulationState.json");
            var simulationState = JsonConvert.DeserializeObject<SimulationState[]>(stateJson);
            var resultsToAdd = new ConcurrentDictionary<IStrategy, SimulationState[]>();
            resultsToAdd.TryAdd(strategy, simulationState);

            _target.AddResults(investor, resultsToAdd);
        }

        [Test]
        public async Task PerformanceChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "ChartTests", "PerformanceChart.png");

            await _target.SaveChart(ResultsChart.Performance, path);

            Approvals.Verify(new FileInfo(path));
        }

        [Test]
        public async Task RelativeChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "ChartTests", "RelativeChart.png");

            await _target.SaveChart(ResultsChart.Relative, path);

            Approvals.Verify(new FileInfo(path));
        }

        [Test]
        public async Task SignalChart()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "ChartTests", "SignalChart.png");

            await _target.SaveChart(ResultsChart.Signal, path);

            Approvals.Verify(new FileInfo(path));
        }

        [Test]
        [UseApprovalSubdirectory("StatisticalTests")]
        public void PriceProbabilities()
        {
            var currentPrice = _target.CurrentPrice();
            var dailyProbability = _target.PriceProbability(Period.Day);
            var weeklyProbability = _target.PriceProbability(Period.Week);
            var monthlyProbability = _target.PriceProbability(Period.Month);
            var quarterlyProbability = _target.PriceProbability(Period.Quarter);
            var maximumProbability = _target.MaximumPriceProbability(Period.Day);

            Approvals.Verify(
@$"Current Price: {currentPrice:C2}
Daily: {dailyProbability:P2}
Weekly: {weeklyProbability:P2}
Monthly: {monthlyProbability:P2}
Quarterly: {quarterlyProbability:P2}
Maximum: (Price) {maximumProbability.Price:C2}, (Probability) {maximumProbability.Probability:P2}");
        }

        [Test]
        public async Task SummaryReport_Html()
        {
            var reportProvider = new ReportProvider(_target);

            var (investor, results) = _target.GetResults().First();
            var report = await reportProvider.GenerateReports(investor, results);

            Approvals.VerifyHtml(report.Summary.Body);
        }

        [Test]
        public async Task StrategyReport_Html()
        {
            var reportProvider = new ReportProvider(_target);

            var (investor, results) = _target.GetResults().First();
            var report = await reportProvider.GenerateReports(investor, results);

            Approvals.VerifyHtml(report.StrategyReports[0].Body);
        }

    }
}
