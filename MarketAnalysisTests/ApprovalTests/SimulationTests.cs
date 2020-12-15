using ApprovalTests;
using ApprovalTests.Reporters;
using MarketAnalysis.Strategy.Parameters;
using NUnit.Framework;
using System.Configuration;
using System.Linq;

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
    }
}