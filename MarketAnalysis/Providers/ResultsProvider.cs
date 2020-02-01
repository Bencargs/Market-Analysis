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
        private readonly MarketDataCache _marketDataCache;
        private readonly ProgressBarProvider _progressProvider;
        private readonly IRepository<MarketData> _marketDataRepository;
        private readonly IRepository<SimulationResult> _simulationResultsRepository;
        private SimulationState[] _marketAverage;
        private SimulationState[] _marketMaximum;

        public ResultsProvider(
            ISimulator simulator, 
            MarketDataCache marketDataCache,
            ProgressBarProvider progressProvider,
            IRepository<MarketData> marketDataRepository,
            IRepository<SimulationResult> simulationResultsRepository)
        {
            _marketDataCache = marketDataCache;
            _simulator = simulator;
            _progressProvider = progressProvider;
            _marketDataRepository = marketDataRepository;
            _simulationResultsRepository = simulationResultsRepository;
        }

        public void Initialise()
        {
            var buyDates = _marketDataCache.TakeUntil().Select(x => x.Date).ToDictionary(k => k, v => true);
            var constantStrategy = new StaticDatesStrategy(buyDates);
            _marketAverage = _simulator.Evaluate(constantStrategy).ToArray();
            _marketMaximum = GetMarketMaximum(_simulator, buyDates).ToArray();
        }

        public void AddResults(Dictionary<IStrategy, SimulationState[]> source)
        {
            foreach (var s in source)
            {
                var history = s.Value.ToArray();
                var latestState = history.Last();
                var simulationDays = history.Length - _marketDataCache.BacktestingIndex;
                var profitTotal = latestState.Worth - GetInvestmentSince(simulationDays);
                var buySignals = history.Where(x => x.ShouldBuy).ToArray();
                var confusionMatrix = CalculateConfusionMatrix(history);
                var excessReturns = GetExcessReturns(history);
                var maximumReturn = _marketMaximum.Last().Worth;
                var currentMarketWorth = _marketAverage.Last().Worth;

                _results.Add(new SimulationResult
                {
                    Date = latestState.Date,
                    SimulationDays = simulationDays,
                    ShouldBuy = latestState.ShouldBuy,
                    ProfitTotal = profitTotal,
                    ProfitYTD = CalculateYTDProfit(history),
                    AboveMarketReturn = excessReturns.Last(),
                    Alpha = CalculateAlpha(latestState.Worth, currentMarketWorth),
                    MaximumAlpha = CalculateAlpha(maximumReturn, currentMarketWorth),
                    MaximumDrawdown = excessReturns.Min(),
                    BuyCount = buySignals.Count(),
                    MaximumHoldingPeriod = GetMaximumHoldPeriod(history, buySignals),
                    SharpeRatio = CalculateSharpeRatio(history, currentMarketWorth),
                    MarketCorrelation = CalculateCorrelation(history),
                    Accuracy = CalculateAccuracy(confusionMatrix, history),
                    Recall = CalculateRecall(confusionMatrix),
                    Precision = CalculatePrecision(confusionMatrix),
                    ConfusionMatrix = confusionMatrix,
                    AverageReturn = GetAverageReturn(buySignals)
                }.SetStrategy(s.Key));
            }
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

        private int GetMaximumHoldPeriod(SimulationState[] history, SimulationState[] buySignals)
        {
            if (!buySignals.Any())
                return history.Length;

            var buyIndexes = buySignals.Select(x => Array.IndexOf(history, x)).ToArray();

            return buyIndexes.Skip(1).Select((x, i) => x > 0 ? x - buyIndexes[i] : x).Max();
        }

        private IEnumerable<SimulationState> GetMarketMaximum(ISimulator simulator, Dictionary<DateTime, bool> buyDates)
        {
            var strategy = new StaticDatesStrategy(buyDates);
            var history = simulator.Evaluate(strategy);
            var worth = history.LastOrDefault()?.Worth ?? 0m;

            var i = 1;
            using (var progressBar = _progressProvider.Create(_marketDataCache.BacktestingIndex, "Initialising..."))
            {
                foreach (var (date, _) in buyDates.Where(x => x.Key > Configuration.BacktestingDate).Reverse())
                {
                    buyDates[date] = false;
                    strategy = new StaticDatesStrategy(buyDates) { Identifier = i++ };
                    var newHistory = simulator.Evaluate(strategy);
                    var newWorth = newHistory?.LastOrDefault()?.Worth ?? 0m;
                    if (newWorth < worth)
                        buyDates[date] = true;
                    else
                    {
                        worth = newWorth;
                        history = newHistory;
                    }
                    progressBar.Tick($"Initialising...");
                }
            }
            return history;
        }

        private decimal GetAverageReturn(SimulationState[] buySignals)
        {
            var signalReturns = GetExcessReturns(buySignals);
            if (!signalReturns.Any())
                return 0m;

            return signalReturns.Skip(1).Select((x, i) => x - signalReturns[i]).Average();
        }

        private double CalculateCorrelation(IList<SimulationState> history)
        {
            var modeled = _marketAverage.Select(x => (double)x.Worth).ToArray();
            var observed = history.Select(x => (double)x.Worth).ToArray();

            return MathNet.Numerics.GoodnessOfFit.RSquared(modeled, observed);
        }

        private decimal CalculateSharpeRatio(IList<SimulationState> history, decimal currentMarketWorth)
        {
            var riskFreeRate = history.Last().Worth - currentMarketWorth;

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
            var denominator = confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalsePostative];

            return denominator != 0 ? (decimal) confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
        }

        private decimal CalculateRecall(Dictionary<ConfusionCategory, int> confusionMatrix)
        {
            var denominator = confusionMatrix[ConfusionCategory.TruePostative] + confusionMatrix[ConfusionCategory.FalseNegative];

            return denominator != 0 ? (decimal) confusionMatrix[ConfusionCategory.TruePostative] / denominator : 0;
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

        private decimal CalculateAlpha(decimal currentWorth, decimal currentMarketWorth)
        {
            var excessReturn = currentWorth - currentMarketWorth;
            return excessReturn / currentMarketWorth;
        }

        private decimal CalculateYTDProfit(IList<SimulationState> history)
        {
            var latestState = history.Last();
            var yearOpenDay = history.ToList().FindIndex(x => x.Date > new DateTime(latestState.Date.Year, 1, 1));
            var strategyOpenWorth = history[yearOpenDay].Worth;
            var investment = GetInvestmentSince(history.Count - yearOpenDay);
            return latestState.Worth - strategyOpenWorth - investment;
        }

        private decimal GetInvestmentSince(int simulationDays)
        {
            return simulationDays * Configuration.DailyFunds;
        }
    }
}
