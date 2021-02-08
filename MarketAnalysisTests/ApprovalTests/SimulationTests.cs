using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using MarketAnalysis.Strategy.Parameters;
using NUnit.Framework;
using System.Configuration;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;

namespace MarketAnalysisTests.ApprovalTests
{
    [UseReporter(typeof(DiffReporter))]
    public class SimulationTests : TestHarness
    {
        [SetUp]
        public void Setup()
        {
            ConfigurationManager.AppSettings["BacktestingDate"] = "2010-07-01";
            ConfigurationManager.AppSettings["CacheSize"] = "2000";
            ConfigurationManager.AppSettings["DataPath"] = "MarketData.csv";
        }

        [Test]
        public void StaticDaysTest()
        {
            var data = CreateMarketData();
            var parameters = new StaticDatesParameters
            {
                BuyDates = data.ToDictionary(k => k.Date, v => true)
            };
            
            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);
            
            Approvals.Verify(actual);
        }

        [Test]
        public void DeltaStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new DeltaParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void GradientStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new GradientParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void LinearRegressionStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new LinearRegressionParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void RelativeStrengthStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new RelativeStrengthParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void VolumeStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new VolumeParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void MovingAverageStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new MovingAverageParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void EntropyStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new EntropyParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void HolidayEffectStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new HolidayEffectParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }
        
        [Test]
        public void OptimalStoppingStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new OptimalStoppingParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void ProbabilityStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new ProbabilityParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void SpreadStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new SpreadParameters();

            var target = SimulateStrategy(data, x => x.Create(parameters));
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void WeightedStrategyTest()
        {
            var data = CreateMarketData();
            var investor = new Investor { DailyFunds = 10, OrderDelayDays = 3 };
            var investorProvider = CreateInvestorProvider(investor);
            var marketDataCache = CreateMarketDataCache(data);
            var simulationCache = new SimulationCache();
            var simulationFactory = new SimulatorFactory(marketDataCache, simulationCache);
            var strategyFactory = CreateStrategyFactory(marketDataCache, simulationCache, investorProvider);
            var deltaStrategy = strategyFactory.Create(new DeltaParameters());
            var volumeStrategy = strategyFactory.Create(new VolumeParameters());
            var _ = simulationFactory.Create<BacktestingSimulator>()
                .Evaluate(deltaStrategy, investor).ToArray();
            var __ = simulationFactory.Create<BacktestingSimulator>()
                .Evaluate(volumeStrategy, investor).ToArray();

            var parameters = new WeightedParameters
            {
                Weights = new Dictionary<IStrategy, double>
                {
                    { deltaStrategy, 0d },
                    { volumeStrategy, 0d }
                }
            };
            var strategy = strategyFactory.Create(parameters);
            var target = simulationFactory.Create<BacktestingSimulator>()
                .Evaluate(strategy, investor);
            
            var actual = ToApprovedString(target);
            Approvals.Verify(actual);
        }
    }
}