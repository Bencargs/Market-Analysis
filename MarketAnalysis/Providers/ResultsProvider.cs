using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Providers
{
    public class ResultsProvider : IResultsProvider
    {
        private readonly List<SimulationResult> _results = new List<SimulationResult>(5000);
        private readonly ISimulator _simulator;
        private readonly MarketDataCache _cache;
        private SimulationState[] _marketAverage;
        private SimulationState[] _marketMaximum;

        public ResultsProvider(ISimulator simulator, MarketDataCache cache)
        {
            _cache = cache;
            _simulator = simulator;
        }

        public void Initialise()
        {
            _marketAverage = _simulator.Evaluate(new ConstantStrategy(), showProgress: false).ToArray();
            _marketMaximum = GetMarketMaximum(_simulator).ToArray();
        }

        private IEnumerable<SimulationState> GetMarketMaximum(ISimulator simulator)
        {
            using (var progress = ProgressBarReporter.StartProgressBar(_cache.BacktestingIndex, "Initialising..."))
            {
                var buyDates = _cache.TakeUntil().Select(x => x.Date).ToDictionary(k => k, v => true);
                var strategy = new StaticDatesStrategy(buyDates);
                var history = simulator.Evaluate(strategy, showProgress: false);
                var worth = history.LastOrDefault()?.Worth ?? 0m;

                var i = 0;
                foreach (var (date, _) in buyDates.Where(x => x.Key > Configuration.BacktestingDate).Reverse())
                {
                    buyDates[date] = false;
                    strategy = new StaticDatesStrategy(buyDates) { Identifier = i++ };
                    var newHistory = simulator.Evaluate(strategy, showProgress: false);
                    var newWorth = newHistory?.LastOrDefault()?.Worth ?? 0m;
                    if (newWorth < worth)
                        buyDates[date] = true;
                    else
                    {
                        worth = newWorth;
                        history = newHistory;
                    }
                    progress.Tick($"Initialising...");
                }

                return history;
            }
        }

        public void AddResults(IStrategy strategy, IEnumerable<SimulationState> source)
        {
            var history = source.ToArray();
            var latestState = history.Last();
            var currentMarketWorth = _marketAverage.Last().Worth;

            var profitTotal = latestState.Worth - GetInvestmentSince(_cache.BacktestingIndex, history);
            var profitYTD = CalculateYTDProfit(history);

            var aboveMarketReturn = latestState.Worth - currentMarketWorth;

            var alpha = CalculateAlpha(latestState.Worth);
            var maximumAlpha = CalculateAlpha(_marketMaximum.Last().Worth);

            var maximumDrawdown = CalculateMaximumDrawdown(history);

            var buyCount = history.Count(x => x.ShouldBuy);

            var sharpeRatio = CalculateSharpeRatio(history);

            var confusionMatrix = CalculateConfusionMatrix(history);
            var accuracy = CalculateAccuracy(confusionMatrix, history);
            var recall = CalculateRecall(confusionMatrix);
            var precision = CalculatePrecision(confusionMatrix);

            _results.Add(new SimulationResult
            {
                Date = latestState.Date,
                ShouldBuy = latestState.ShouldBuy,
                ProfitTotal = profitTotal,
                ProfitYTD = profitYTD,
                AboveMarketReturn = aboveMarketReturn,
                Alpha = alpha,
                MaximumAlpha = maximumAlpha,
                MaximumDrawdown = maximumDrawdown,
                BuyCount = buyCount,
                SharpeRatio = sharpeRatio,
                Accuracy = accuracy,
                Recall = recall,
                Precision = precision,
                ConfusionMatrix = confusionMatrix
            }.SetStrategy(strategy));
        }

        public IEnumerable<SimulationResult> GetResults()
        {
            return _results;
        }

        public bool ShouldBuy()
        {
            return _results.Count(x => x.ShouldBuy) > 1;
        }

        public decimal TotalProfit()
        {
            return _results.Sum(x => x.ProfitTotal) / _results.Count();
        }

        private decimal CalculateSharpeRatio(IList<SimulationState> history)
        {
            var riskFreeRate = history.Last().Worth - _marketAverage.Last().Worth;

            var startIndex = Array.FindIndex(_marketAverage, x => x.Date == history.First().Date);
            var excessReturns = history.Select((x, i) => x.Worth - _marketAverage[startIndex + i].Worth).ToArray();
            var meanReturn = excessReturns.Average();

            var averageSum = excessReturns.Select(x => Math.Pow((double)(x - meanReturn), 2));
            var stdvn = (decimal) Math.Sqrt( averageSum.Average() );

            return stdvn != 0 
                ? riskFreeRate / stdvn
                : 0;
        }

        private decimal CalculatePrecision(Dictionary<ConfusionCategory, int> confusionMatrix)
        {
            var denominator = (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalsePostative]);
            return (decimal) denominator != 0 ? confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
        }

        private decimal CalculateRecall(Dictionary<ConfusionCategory, int> confusionMatrix)
        {
            var denominator = (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalseNegative]);
            return (decimal) denominator != 0 ? confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
        }

        private decimal CalculateAccuracy(Dictionary<ConfusionCategory, int> confusionMatrix, IList<SimulationState> history)
        {
            return (decimal) (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.TrueNegative]) / history.Count;
        }

        private Dictionary<ConfusionCategory, int> CalculateConfusionMatrix(IList<SimulationState> history)
        {
            var confusionMatrix = new Dictionary<ConfusionCategory, int>
            {
                { ConfusionCategory.TruePostative, 0 },
                { ConfusionCategory.FalsePostative, 0 },
                { ConfusionCategory.TrueNegative, 0 },
                { ConfusionCategory.FalseNegative, 0 }
            };

            var optimalBuys = _marketMaximum.Where(x => x.ShouldBuy).Select(x => x.Date).ToArray();
            var strategyBuys = history.Where(x => x.ShouldBuy).Select(x => x.Date).ToArray();
            foreach (var h in history)
            {
                var ideal = optimalBuys.Contains(h.Date);
                var model = strategyBuys.Contains(h.Date);

                if (model && ideal)
                    confusionMatrix[ConfusionCategory.TruePostative]++;
                if (!model && !ideal)
                    confusionMatrix[ConfusionCategory.TrueNegative]++;
                if (model && !ideal)
                    confusionMatrix[ConfusionCategory.FalsePostative]++;
                if (!model && ideal)
                    confusionMatrix[ConfusionCategory.FalseNegative]++;
            }

            return confusionMatrix;
        }

        private decimal CalculateMaximumDrawdown(IList<SimulationState> history)
        {
            return history.Select((h, i) => h.Worth - _marketAverage.First(x => x.Date == h.Date).Worth).Min();
        }

        private decimal CalculateAlpha(decimal currentWorth)
        {
            var currentMarketWorth = _marketAverage.Last().Worth;
            var excessReturn = currentWorth - currentMarketWorth;
            return excessReturn / currentMarketWorth;
        }

        private decimal CalculateYTDProfit(IList<SimulationState> history)
        {
            var latestState = history.Last();
            var yearOpenDay = history.ToList().FindIndex(x => x.Date > new DateTime(latestState.Date.Year, 1, 1));
            var strategyOpenWorth = history[yearOpenDay].Worth;
            var investment = GetInvestmentSince(yearOpenDay, history);
            return (latestState.Worth - strategyOpenWorth) - investment;
        }

        private decimal GetInvestmentSince(int index, IList<SimulationState> history)
        {
            return (history.Count - index) * Configuration.DailyFunds;
        }
    }
}
