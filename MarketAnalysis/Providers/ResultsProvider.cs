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

        public void Initialise(IEnumerable<Row> data)
        {
            var marketData = data.ToArray();
            var simulator = new Simulator(data, false);

            _marketAverage = simulator.Evaluate(new ConstantStrategy());
            _marketMaximum = GetMarketMaximum(simulator, marketData);
        }

        private List<SimulationState> GetMarketMaximum(ISimulator simulator, Row[] data)
        {
            using (var progress = ProgressBarReporter.StartProgressBar(data.Count(), "Initialising..."))
            {
                SimulationCache.Instance.IsEnabled = false; //todo - fix this
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
                SimulationCache.Instance.IsEnabled = true;
                return history;
            }
        }

        public void AddResults(IStrategy strategy, List<SimulationState> history)
        {
            var latestState = history.Last();
            var currentMarketWorth = _marketAverage.Last().Worth;

            var profitTotal = CalculateTotalProfit(latestState);
            var profitYTD = CalculateYTDProfit(history, currentMarketWorth);

            var alpha = CalculateAlpha(currentMarketWorth);
            var maximumAlpha = CalculateAlpha(_marketMaximum.Last().Worth);

            var maximumDrawdown = CalculateMaximumDrawdown(history);

            var frequency = CalculateFrequency(history);

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
                Alpha = alpha,
                MaximumAlpha = maximumAlpha,
                MaximumDrawdown = maximumDrawdown,
                Frequency = frequency,
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
            return (decimal) confusionMatrix[ConfusionCategory.TruePostative]
                            / (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalsePostative]);
        }

        private decimal CalculateRecall(Dictionary<ConfusionCategory, int> confusionMatrix, List<SimulationState> history)
        {
            return (decimal)confusionMatrix[ConfusionCategory.TruePostative] 
                / (confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalseNegative]);
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
                else if (!model && ideal)
                    confusionMatrix[ConfusionCategory.FalsePostative]++;
                else if (model && !ideal)
                    confusionMatrix[ConfusionCategory.TrueNegative]++;
                else if (!model && !ideal)
                    confusionMatrix[ConfusionCategory.FalseNegative]++;
            }

            return confusionMatrix;
        }

        private decimal CalculateFrequency(List<SimulationState> history)
        {
            return (decimal)history.Count(x => x.ShouldBuy) / history.Count();
        }

        private decimal CalculateMaximumDrawdown(List<SimulationState> history)
        {
            return history.Select((h, i) => h.Worth - _marketAverage[i].Worth).Min();
        }

        private decimal CalculateAlpha(decimal currentMarketWorth)
        {
            var excessReturn = currentMarketWorth - _marketAverage.Last().Worth;
            return excessReturn / currentMarketWorth;
        }

        private decimal CalculateYTDProfit(List<SimulationState> history, decimal currentMarketWorth)
        {
            var latestState = history.Last();
            var yearOpenDay = history.First(x => x.Date > new DateTime(latestState.Date.Year, 1, 1)).Date;
            var marketOpenWorth = _marketAverage.First(x => x.Date == yearOpenDay).Worth;
            var strategyOpenWorth = history.First(x => x.Date == yearOpenDay).Worth;
            return (latestState.Worth - strategyOpenWorth) - (currentMarketWorth - marketOpenWorth);
        }

        private decimal CalculateTotalProfit(SimulationState latestState)
        {
            var marketAtDate = _marketAverage.First(x => x.Date == latestState.Date);
            return latestState.Worth - marketAtDate.Worth;
        }
    }
}
