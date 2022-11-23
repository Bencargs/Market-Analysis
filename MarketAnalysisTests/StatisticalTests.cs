using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using MarketAnalysis;
using MarketAnalysis.Strategy.Parameters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysisTests
{
    /// <summary>
    /// Monte Carlo simulation to collect statistical average
    /// Calculates the average daily profit
    /// training and optimisation not implemented, therefor only static strategies considered
    /// </summary>
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("StatisticalTests")]
    public class StatisticalTests : TestHarness
    {
        private static readonly int RandomSeed = 42;
        private static readonly decimal PerDiem = 10;
        private static readonly int DurationDays = 300;
        private static readonly int SimulationCount = 100;
        private static readonly DateTime StartDate = DateTime.Parse("2010-07-01");

        [Test]
        public void MarketAverageProfit()
        {
            var rng = new Random(RandomSeed);
            var data = CreateMarketData().SkipWhile(x => x.Date < StartDate).ToArray();
            var parameters = new StaticDatesParameters
            {
                BuyDates = data.ToDictionary(d => d.Date, v => true)
            };

            var results = new List<decimal[]>();
            for (var i = 0; i < SimulationCount; i++)
            {
                var index = rng.Next(data.Length - DurationDays);
                var subset = data[index..(index + DurationDays)];

                var target = SimulateStrategy(subset, x => x.Create(parameters)).ToArray();

                results.Add(target.Select(x => x.Worth).ToArray());
            }

            var average = results.Select(x => x.Last()).Average();
            var dailyYieldRate = average / DurationDays;
            var averageDailyProfit = dailyYieldRate - PerDiem;

            Approvals.Verify(averageDailyProfit);
        }

        [Test]
        public void DeltaAverageProfit()
        {
            var rng = new Random(RandomSeed);
            var data = CreateMarketData().SkipWhile(x => x.Date < StartDate).ToArray();
            var parameters = new DeltaParameters();

            var results = new List<decimal[]>();
            for (var i = 0; i < SimulationCount; i++)
            {
                var index = rng.Next(data.Length - DurationDays);
                var subset = data[index..(index + DurationDays)];

                var target = SimulateStrategy(subset, x => x.Create(parameters)).ToArray();

                results.Add(target.Select(x => x.Worth).ToArray());
            }

            var average = results.Select(x => x.Last()).Average();
            var dailyYieldRate = average / DurationDays;
            var averageDailyProfit = dailyYieldRate - PerDiem;

            Approvals.Verify(averageDailyProfit);
        }

        [Test]
        public void OptimalStoppingAverageProfit()
        {
            var rng = new Random(RandomSeed);
            var data = CreateMarketData().SkipWhile(x => x.Date < StartDate).ToArray();
            var parameters = new OptimalStoppingParameters();

            var results = new List<decimal[]>();
            for (var i = 0; i < SimulationCount; i++)
            {
                var index = rng.Next(data.Length - DurationDays);
                var subset = data[index..(index + DurationDays)];

                var target = SimulateStrategy(subset, x => x.Create(parameters)).ToArray();

                results.Add(target.Select(x => x.Worth).ToArray());
            }

            var average = results.Select(x => x.Last()).Average();
            var dailyYieldRate = average / DurationDays;
            var averageDailyProfit = dailyYieldRate - PerDiem;

            Approvals.Verify(averageDailyProfit);
        }

        [Test]
        public void HolidayEffectAverageProfit()
        {
            var rng = new Random(RandomSeed);
            var data = CreateMarketData().SkipWhile(x => x.Date < StartDate).ToArray();
            var parameters = new HolidayEffectParameters();

            var results = new List<decimal[]>();
            for (var i = 0; i < SimulationCount; i++)
            {
                var index = rng.Next(data.Length - DurationDays);
                var subset = data[index..(index + DurationDays)];

                var target = SimulateStrategy(subset, x => x.Create(parameters)).ToArray();

                results.Add(target.Select(x => x.Worth).ToArray());
            }

            var average = results.Select(x => x.Last()).Average();
            var dailyYieldRate = average / DurationDays;
            var averageDailyProfit = dailyYieldRate - PerDiem;

            Approvals.Verify(averageDailyProfit);
        }

        [Test]
        public void SpreadAverageProfit()
        {
            var rng = new Random(RandomSeed);
            var data = CreateMarketData().SkipWhile(x => x.Date < StartDate).ToArray();
            var parameters = new SpreadParameters();

            var results = new List<decimal[]>();
            for (var i = 0; i < SimulationCount; i++)
            {
                var index = rng.Next(data.Length - DurationDays);
                var subset = data[index..(index + DurationDays)];

                var target = SimulateStrategy(subset, x => x.Create(parameters)).ToArray();

                results.Add(target.Select(x => x.Worth).ToArray());
            }

            var average = results.Select(x => x.Last()).Average();
            var dailyYieldRate = average / DurationDays;
            var averageDailyProfit = dailyYieldRate - PerDiem;

            Approvals.Verify(averageDailyProfit);
        }
    }
}
