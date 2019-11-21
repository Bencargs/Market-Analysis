using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Providers
{
    public class ResultsProvider : IResultsProvider
    {
        private List<SimulationResult> _results = new List<SimulationResult>();
        private List<SimulationState> _marketAverage;
        private List<SimulationState> _marketMaximum;

        public void Initialise(IEnumerable<MarketData> data)
        {
            var marketData = data.ToArray();
            var simulator = new Simulator(data, false);

            _marketAverage = simulator.Evaluate(new ConstantStrategy());
            _marketMaximum = GetMarketMaximum(simulator, marketData);
        }

        private List<SimulationState> GetMarketMaximum(ISimulator simulator, MarketData[] data)
        {
            using (var progress = ProgressBarReporter.StartProgressBar(data.Count(), "Initialising..."))
            using (new CacheSettings { IsEnabled = false })
            {
                var buyDates = Enumerable.Repeat(true, data.Count()).ToArray();
                var strategy = new StaticDatesStrategy(buyDates);
                var history = simulator.Evaluate(strategy);
                var worth = history.LastOrDefault()?.Worth ?? 0m;

                for (int i = buyDates.Length - 1; i > 0; i--)
                {
                    buyDates[i] = false;
                    strategy = new StaticDatesStrategy(buyDates);
                    var newHistory = simulator.Evaluate(strategy);
                    var newWorth = newHistory?.LastOrDefault()?.Worth ?? 0m;
                    if (newWorth < worth)
                        buyDates[i] = true;
                    else
                    {
                        worth = newWorth;
                        history = newHistory;
                    }
                    progress.Tick($"Initialising... x:{data.Count() - i}");
                }
                return history;
            }
        }

        public void AddResults(IStrategy strategy, List<SimulationState> history)
        {
            var latestState = history.Last();
            var currentMarketWorth = _marketAverage.Last().Worth;

            var profitTotal = latestState.Worth - GetInvestmentSince(0, history);
            var profitYTD = CalculateYTDProfit(history);

            var aboveMarketReturn = latestState.Worth - currentMarketWorth;

            var alpha = CalculateAlpha(latestState.Worth);
            var maximumAlpha = CalculateAlpha(_marketMaximum.Last().Worth);

            var maximumDrawdown = CalculateMaximumDrawdown(history);

            var buyCount = history.Count(x => x.ShouldBuy);

            var confusionMatrix = CalculateConfusionMatrix(history);
            var accuracy = CalculateAccuracy(confusionMatrix, history);
            var recall = CalculateRecall(confusionMatrix, history);
            var precision = CalculatePrecision(confusionMatrix, history);

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

        private decimal CalculatePrecision(Dictionary<ConfusionCategory, int> confusionMatrix, List<SimulationState> history)
        {
            var denominator = (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalsePostative]);
            return (decimal) denominator != 0 ? confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
        }

        private decimal CalculateRecall(Dictionary<ConfusionCategory, int> confusionMatrix, List<SimulationState> history)
        {
            var denominator = (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalseNegative]);
            return (decimal) denominator != 0 ? confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
        }

        private decimal CalculateAccuracy(Dictionary<ConfusionCategory, int> confusionMatrix, List<SimulationState> history)
        {
            return (decimal) (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.TrueNegative]) / history.Count;
        }

        private Dictionary<ConfusionCategory, int> CalculateConfusionMatrix(List<SimulationState> history)
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

        private decimal CalculateMaximumDrawdown(List<SimulationState> history)
        {
            return history.Select((h, i) => h.Worth - _marketAverage[i].Worth).Min();
        }

        private decimal CalculateAlpha(decimal currentWorth)
        {
            var currentMarketWorth = _marketAverage.Last().Worth;
            var excessReturn = currentWorth - currentMarketWorth;
            return excessReturn / currentMarketWorth;
        }

        private decimal CalculateYTDProfit(List<SimulationState> history)
        {
            var latestState = history.Last();
            var yearOpenDay = history.FindIndex(x => x.Date > new DateTime(latestState.Date.Year, 1, 1));
            var strategyOpenWorth = history[yearOpenDay].Worth;
            var investment = GetInvestmentSince(yearOpenDay, history);
            return (latestState.Worth - strategyOpenWorth) - investment;
        }

        private decimal GetInvestmentSince(int index, List<SimulationState> history)
        {
            return (history.Count - index) * Configuration.DailyFunds;
        }
    }
}
