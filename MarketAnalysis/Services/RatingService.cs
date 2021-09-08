using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Services
{
    public class RatingService
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly SimulatorFactory _simulatorFactory;
        private readonly IInvestorProvider _investorProvider;

        private SimulationState[] _marketAverage;
        private SimulationState[] _marketMaximum;

        public RatingService(
            IMarketDataCache marketDataCache,
            SimulatorFactory simulatorFactory,
            IInvestorProvider investorProvider)
        {
            _marketDataCache = marketDataCache;
            _simulatorFactory = simulatorFactory;
            _investorProvider = investorProvider;
        }
        
        public void RateMarketData(DateTime? toDate = null)
        {
            var buyDates = _marketDataCache
                .TakeUntil(toDate)
                .Skip(_marketDataCache.BacktestingIndex)
                .Select(x => x.Date)
                .ToDictionary(k => k, _ => true);

            using var progressBar = ProgressBarProvider.Create(_marketDataCache.BacktestingIndex, "Initialising...");
            _marketAverage = SimulateBuyDates(buyDates);
            _marketMaximum = GetMarketMaximum(buyDates, progressBar);
        }


        public IEnumerable<decimal> GetMarketAverageWorth() => _marketAverage.Select(x => x.Worth);

        public decimal GetMarketAverageWorth(int index) => _marketAverage[index].Worth;

        public SimulationState[] GetMarketMaximum() => _marketMaximum;
        
        public Dictionary<DateTime, decimal> CalculateMarketDataValues(MarketData[] data)
        {
            var buyDates = data.ToDictionary(d => d.Date, _ => true);
            var marketDataValues = GetMarketDataValues(buyDates);

            return marketDataValues;
        }

        private SimulationState[] SimulateBuyDates(Dictionary<DateTime, bool> buyDates)
        {
            var constantStrategy = new StaticDatesStrategy(
                new StaticDatesParameters
                {
                    BuyDates = buyDates
                });

            return _simulatorFactory
                .Create<BacktestingSimulator>()
                .Evaluate(constantStrategy, _investorProvider.Current)
                .ToArray();
        }

        private Dictionary<DateTime, decimal> GetMarketDataValues(Dictionary<DateTime, bool> buyDates)
        {
            Dictionary<DateTime, decimal> buyDateValue = new();
            var marketAverageWorth = SimulateBuyDates(buyDates)?.LastOrDefault()?.Worth ?? 0m;
            foreach (var (date, _) in buyDates)
            {
                buyDates[date] = false;

                var newWorth = SimulateBuyDates(buyDates).Last().Worth;
                var buyValue = newWorth - marketAverageWorth;
                buyDateValue[date] = buyValue;
                
                buyDates[date] = true;
            }

            return buyDateValue;
        }

        private SimulationState[] GetMarketMaximum(Dictionary<DateTime, bool> buyDates, ShellProgressBar.ProgressBar progressBar)
        {
            var history = Array.Empty<SimulationState>();
            var worth = GetMarketAverageWorth().Last();
            
            foreach (var (date, _) in buyDates.Reverse())
            {
                buyDates[date] = false;
                var newHistory = SimulateBuyDates(buyDates);
                var newWorth = newHistory?.LastOrDefault()?.Worth ?? 0m;

                if (newWorth < worth)
                    buyDates[date] = true;
                else
                {
                    worth = newWorth;
                    history = newHistory;
                }
                progressBar?.Tick($"Initialising...");
            }

            return history;
        }
    }
}
