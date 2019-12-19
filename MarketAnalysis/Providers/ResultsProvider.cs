using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class ResultsProvider : IResultsProvider
    {
        private readonly List<SimulationResult> _results = new List<SimulationResult>(5000);
        private readonly ISimulator _simulator;
        private readonly MarketDataCache _cache;
        private readonly IRepository<MarketData> _marketDataRepository;
        private readonly IRepository<SimulationResult> _simulationResultsRepository;
        private SimulationState[] _marketAverage;
        private SimulationState[] _marketMaximum;

        public ResultsProvider(
            ISimulator simulator, 
            MarketDataCache cache, 
            IRepository<MarketData> marketDataRepository,
            IRepository<SimulationResult> simulationResultsRepository)
        {
            _cache = cache;
            _simulator = simulator;
            _marketDataRepository = marketDataRepository;
            _simulationResultsRepository = simulationResultsRepository;
        }

        public void Initialise()
        {
            _marketAverage = _simulator.Evaluate(new ConstantStrategy(), showProgress: false).ToArray();
            _marketMaximum = GetMarketMaximum(_simulator).ToArray();
        }

        public void AddResults(IStrategy strategy, IEnumerable<SimulationState> source)
        {
            var history = source.ToArray();
            var latestState = history.Last();
            var simulationDays = history.Length - _cache.BacktestingIndex;
            var profitTotal = latestState.Worth - GetInvestmentSince(simulationDays, history);
            var buySignals = history.Where(x => x.ShouldBuy).ToArray();
            var confusionMatrix = CalculateConfusionMatrix(history);
            var excessReturns = GetExcessReturns(history);

            _results.Add(new SimulationResult
            {
                Date = latestState.Date,
                SimulationDays = simulationDays,
                ShouldBuy = latestState.ShouldBuy,
                ProfitTotal = profitTotal,
                ProfitYTD = CalculateYTDProfit(history),
                AboveMarketReturn = excessReturns.Last(),
                Alpha = CalculateAlpha(latestState.Worth),
                MaximumAlpha = CalculateAlpha(_marketMaximum.Last().Worth),
                MaximumDrawdown = excessReturns.Min(),
                BuyCount = buySignals.Count(),
                SharpeRatio = CalculateSharpeRatio(history),
                Accuracy = CalculateAccuracy(confusionMatrix, history),
                Recall = CalculateRecall(confusionMatrix),
                Precision = CalculatePrecision(confusionMatrix),
                ConfusionMatrix = confusionMatrix,
                AverageReturn = GetAverageReturn(buySignals)
            }.SetStrategy(strategy));
        }

        public IEnumerable<SimulationResult> GetResults()
        {
            return _results;
        }

        public async Task SaveSimulationResults()
        {
            await _simulationResultsRepository.Save(_results);
        }

        public async Task SaveData(IEnumerable<MarketData> data)
        {
            await _marketDataRepository.Save(data);
        }

        public bool ShouldBuy()
        {
            return _results.Count(x => x.ShouldBuy) > 1;
        }

        public decimal TotalProfit()
        {
            return _results.Sum(x => x.ProfitTotal) / _results.Count();
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

        private decimal GetAverageReturn(SimulationState[] buySignals)
        {
            var signalReturns = GetExcessReturns(buySignals);
            if (!signalReturns.Any())
                return 0m;

            return signalReturns.Skip(1).Select((x, i) => x - signalReturns[i]).Average();
        }

        private decimal CalculateSharpeRatio(IList<SimulationState> history)
        {
            var riskFreeRate = history.Last().Worth - _marketAverage.Last().Worth;

            var excessReturns = GetExcessReturns(history);
            var meanReturn = excessReturns.Average();

            var averageSum = excessReturns.Select(x => Math.Pow((double)(x - meanReturn), 2));
            var stdvn = (decimal) Math.Sqrt( averageSum.Average() );

            return stdvn != 0 
                ? riskFreeRate / stdvn
                : 0;
        }

        private decimal[] GetExcessReturns(IList<SimulationState> history)
        {
            return history.Select((x, i) => i > 0 
                ? x.Worth - _marketAverage[i].Worth
                : x.Worth).ToArray();
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

        private decimal GetInvestmentSince(int simulationDays, IList<SimulationState> history)
        {
            return simulationDays * Configuration.DailyFunds;
        }
    }
}
