using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Services;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MarketAnalysisTests
{
    public class TestHarness
    {
        public TestHarness()
        {

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x["BacktestingDate"]).Returns("2010-07-01");
            configuration.Setup(x => x["CacheSize"]).Returns("2000");
            configuration.Setup(x => x["DataPath"]).Returns("MarketData.csv");
            configuration.Setup(x => x["RelativePath"]).Returns("");
            configuration.Setup(x => x["LogoImagePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\Logo.png");
            configuration.Setup(x => x["WorldImagePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\World.png");
            configuration.Setup(x => x["PhoneImagePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\Phone.png");
            configuration.Setup(x => x["EmailImagePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\Email.png");
            configuration.Setup(x => x["EmailTemplatePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\Template.html");
            configuration.Setup(x => x["StrategyTemplatePath"]).Returns(@"..\..\..\..\MarketAnalysis\Resources\StrategyTemplate.html");

            Configuration.Initialise(configuration.Object);
        }

        protected static string ToApprovedString(IEnumerable<SimulationState> results)
        {
            var approvedText = results.Select(x => 
                $"{x.Date}," +
                $"{x.BuyCount}," +
                $"{Math.Round(x.SharePrice, 5)}," +
                $"{Math.Round(x.TotalFunds, 5)}," +
                $"{Math.Round(x.OrderQueue.Worth(), 5)}," +
                $"{Math.Round(x.Shares, 5)}," +
                $"{Math.Round(x.Worth, 5)}," +
                $"{x.ShouldBuy}");
            return string.Join(Environment.NewLine, approvedText);
        }

        protected static Dictionary<string, SimulationState> SimulateStrategy(
            IEnumerable<MarketData> data,
            IParameters[] parameters)
        {
            var investorProvider = CreateInvestorProvider();

            var marketDataCache = CreateMarketDataCache(data);
            var simulationCache = new SimulationCache();

            var simulatorFactor = new SimulatorFactory(marketDataCache, simulationCache);
            var simulator = simulatorFactor.Create<BacktestingSimulator>();
            var ratingService = new RatingService(marketDataCache, simulatorFactor, investorProvider);
            var strategyFactory = CreateStrategyFactory(marketDataCache, simulationCache, investorProvider, ratingService);

            var results = new Dictionary<string, SimulationState>();
            foreach (var p in parameters)
            {
                var strategy = strategyFactory.Create(p);
                var state = simulator.Evaluate(strategy, investorProvider.Current);
                results[strategy.StrategyType.GetDescription()] = state.Last();
            }

            return results;
        }

        protected static IEnumerable<SimulationState> SimulateStrategy(
            IEnumerable<MarketData> data,
            Func<StrategyFactory, IStrategy> createStrategyFunc,
            bool initialiseRatingService = false)
        {
            var investorProvider = CreateInvestorProvider();

            var marketDataCache = CreateMarketDataCache(data);
            var simulationCache = new SimulationCache();

            var simulatorFactor = new SimulatorFactory(marketDataCache, simulationCache);
            var simulator = simulatorFactor.Create<BacktestingSimulator>();
            var ratingService = new RatingService(
                marketDataCache,
                simulatorFactor,
                investorProvider);
            var strategyFactory = CreateStrategyFactory(
                marketDataCache, 
                simulationCache, 
                investorProvider, 
                ratingService);
            var strategy = createStrategyFunc(strategyFactory);

            if (initialiseRatingService)
            {
                ratingService.RateMarketData();
            }

            return simulator.Evaluate(strategy, investorProvider.Current);
        }
        
        protected static StrategyFactory CreateStrategyFactory(
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache,
            IInvestorProvider investorProvider,
            RatingService ratingService)
        => new (marketDataCache, simulationCache, investorProvider, ratingService);

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

        protected static IInvestorProvider CreateInvestorProvider()
        {
            var investor = new Investor { DailyFunds = 10, OrderDelayDays = 3 };
            var investorProvider = new Mock<IInvestorProvider>();
            investorProvider.Setup(x => x.Current).Returns(investor);
            return investorProvider.Object;
        }
    }
}
