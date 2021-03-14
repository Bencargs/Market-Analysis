using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Services;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using NUnit.Framework;

namespace MarketAnalysisTests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public class SimulationTests : TestHarness
    {
        [SetUp]
        public void Setup()
        {
            ConfigurationManager.AppSettings["BacktestingDate"] = "2010-07-01";
            ConfigurationManager.AppSettings["CacheSize"] = "2000";
            ConfigurationManager.AppSettings["DataPath"] = "MarketData.csv";
            ConfigurationManager.AppSettings["RelativePath"] = @"..\..\..\";
        }

        [Test]
        public void Test()
        {
            ConfigurationManager.AppSettings["DataPath"] = "MarketData2021-1-15.csv";
            var data = CreateMarketData();
            var parameters = new IParameters[]
            {
                new GradientParameters(),
                new LinearRegressionParameters(),
                new HolidayEffectParameters(),
                new VolumeParameters(),
                new MovingAverageParameters(),
                new DeltaParameters(),
                new RelativeStrengthParameters(),
                new OptimalStoppingParameters()
            };

            var target = SimulateStrategy(data, parameters);

            var actual = string.Join(Environment.NewLine,
                target.Select(k =>
                    $"{k.Key},{k.Value.ShouldBuy},{k.Value.Worth},{k.Value.BuyCount}"));
            Approvals.Verify(actual);
        }

        [Test]
        public void StaticDaysTest()
        {
            var data = CreateMarketData().ToArray();
            var parameters = new StaticDatesParameters
            {
                BuyDates = data.ToDictionary(k => k.Date, _ => true)
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
        public void ClusteringStrategyTest()
        {
            var data = CreateMarketData();
            var parameters = new ClusteringParameters();
            
            var target = SimulateStrategy(
                data, 
                x => x.Create(parameters), 
                true);
            var actual = ToApprovedString(target);

            Approvals.Verify(actual);
        }

        [Test]
        public void WeightedStrategyTest()
        {
            var data = CreateMarketData();
            var investorProvider = CreateInvestorProvider();
            var marketDataCache = CreateMarketDataCache(data);
            var simulationCache = new SimulationCache();
            var simulationFactory = new SimulatorFactory(marketDataCache, simulationCache);
            var ratingService = new RatingService(marketDataCache, simulationFactory, investorProvider);
            var strategyFactory = CreateStrategyFactory(marketDataCache, simulationCache, investorProvider, ratingService);
            var deltaStrategy = strategyFactory.Create(new DeltaParameters());
            var volumeStrategy = strategyFactory.Create(new VolumeParameters());
            var _ = simulationFactory.Create<BacktestingSimulator>()
                .Evaluate(deltaStrategy, investorProvider.Current).ToArray();
            var __ = simulationFactory.Create<BacktestingSimulator>()
                .Evaluate(volumeStrategy, investorProvider.Current).ToArray();

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
                .Evaluate(strategy, investorProvider.Current);

            var actual = ToApprovedString(target);
            Approvals.Verify(actual);
        }
    }
}