using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysisTests.ApprovalTests
{
    public class TestHarness
    {
        protected static string ToApprovedString(IEnumerable<SimulationState> results)
        {
            var approvedText = results.Select(x => 
                $"{x.Date}," +
                $"{x.BuyCount}," +
                $"{Math.Round(x.SharePrice, 5)}," +
                $"{Math.Round(x.Funds, 5)}," +
                $"{Math.Round(x.Orders, 5)}," +
                $"{Math.Round(x.Shares, 5)}," +
                $"{Math.Round(x.Worth, 5)}," +
                $"{x.ShouldBuy}");
            return string.Join(Environment.NewLine, approvedText);
        }

        protected static IEnumerable<SimulationState> SimulateStrategy(
            IEnumerable<MarketData> data,
            Func<StrategyFactory, IStrategy> createStrategyFunc)
        {
            var investor = new Investor { DailyFunds = 10, OrderDelayDays = 3 };
            var investorProvider = CreateInvestorProvider(investor);

            var marketDataCache = CreateMarketDataCache(data);
            var simulationCache = new SimulationCache();

            var simulator = new BacktestingSimulator(marketDataCache, simulationCache);
            var strategyFactory = CreateStrategyFactory(marketDataCache, simulationCache, investorProvider);
            var strategy = createStrategyFunc(strategyFactory);

            return simulator.Evaluate(strategy, investor);
        }

        protected static StrategyFactory CreateStrategyFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider)
        {

            var optimiserFactory = new OptimiserFactory(marketDataCache, simulationCache, investorProvider);
            var strategyFactory = new StrategyFactory(marketDataCache, optimiserFactory);
            return strategyFactory;
        }

        protected static IMarketDataCache CreateMarketDataCache(IEnumerable<MarketData> data)
        {
            var marketDataCache = new MarketDataCache();
            marketDataCache.Initialise(data);
            return marketDataCache;
        }

        protected static IEnumerable<MarketData> CreateMarketData()
        {
            var fileProvider = new FileRepository();
            var data = ((IRepository<MarketData>)fileProvider).Get();
            return data.Result;
        }

        protected static IInvestorProvider CreateInvestorProvider(Investor investor)
        {
            var investorProvider = new Mock<IInvestorProvider>();
            investorProvider.Setup(x => x.Current).Returns(investor);
            return investorProvider.Object;
        }
    }
}
