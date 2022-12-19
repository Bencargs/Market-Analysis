using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using MarketAnalysis.Strategy;
using OxyPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketAnalysis.Services;

namespace MarketAnalysis.Providers
{
    public class ResultsProvider : IResultsProvider
    {
        private readonly List<SimulationResult> _results = new(5000);
        private readonly Dictionary<ResultsChart, Chart> _charts = new();
        private readonly IMarketDataCache _marketDataCache;
        private readonly IRepository<MarketData> _marketDataRepository;
        private readonly RatingService _ratingService;
        private readonly MarketStatisticsProvider _marketStatisticsProvider;
        private readonly IRepository<SimulationResult> _simulationResultsRepository;

        public ResultsProvider(
            IMarketDataCache marketDataCache,
            IRepository<MarketData> marketDataRepository,
            RatingService ratingService,
            MarketStatisticsProvider marketStatisticsProvider,
            IRepository<SimulationResult> simulationResultsRepository)
        {
            _marketDataCache = marketDataCache;
            _marketDataRepository = marketDataRepository;
            _ratingService = ratingService;
            _marketStatisticsProvider = marketStatisticsProvider;
            _simulationResultsRepository = simulationResultsRepository;
        }

        public void Initialise()
        {
            _ratingService.RateMarketData();
            _marketStatisticsProvider.Initialise();

            _charts.Add(ResultsChart.Performance, new Chart("Strategy returns", "Return ($ AU)", "Time (Days)"));
            _charts[ResultsChart.Performance].AddSeries(
                _ratingService
                .GetMarketAverageWorth()
                .Select(x => (double)x), 
                "Market Average", colour: OxyColor.FromArgb(255, 0, 0, 0));

            _charts.Add(ResultsChart.Signal, new Chart("Strategy Buy Signal", "Time (Days)", "Market Price"));
            _charts[ResultsChart.Signal].AddSeries(
                _ratingService.GetMarketAverageWorth()
                    .Select(x => (double)x), 
                "Market Average", colour: OxyColor.FromArgb(255, 0, 0, 0));

            _charts.Add(ResultsChart.Relative, new Chart("Performance vs Market Average", "Profit/Loss ($ AU)", "Time (Days)"));
        }
        
        public void AddResults(Investor investor, ConcurrentDictionary<IStrategy, SimulationState[]> source)
        {
            foreach (var (strategy, simulationResults) in OrderResults(source))
            {
                var history = simulationResults.ToArray();
                var latestState = history.Last();
                var simulationDays = history.Length - _marketDataCache.BacktestingIndex;
                var profitTotal = latestState.Worth - GetInvestmentSince(investor.DailyFunds, simulationDays);
                var buySignals = history.Where(x => x.ShouldBuy).ToArray();
                var confusionMatrix = CalculateConfusionMatrix(history);
                var excessReturns = GetExcessReturns(history);
                var maximumReturn = _ratingService.GetMarketMaximum().Last().Worth;
                var currentMarketWorth = _ratingService.GetMarketAverageWorth().Last();

                AddPerformanceChartSeries(history, strategy);
                AddSignalsChartSeries(history, strategy);
                AddRelativeChartSeries(history, strategy);

                _results.Add(new SimulationResult
                {
                    Date = latestState.Date,
                    Investor = investor,
                    SimulationDays = simulationDays,
                    ShouldBuy = latestState.ShouldBuy,
                    Stake = latestState.OrderQueue.LatestPayment(),
                    TotalFunds = latestState.TotalFunds,
                    ProfitTotal = profitTotal,
                    ProfitYTD = CalculateYTDProfit(investor, history),
                    AboveMarketReturn = excessReturns.Last(),
                    Alpha = CalculateAlpha(latestState.Worth, currentMarketWorth),
                    MaximumAlpha = CalculateAlpha(maximumReturn, currentMarketWorth),
                    MaximumDrawdown = excessReturns.Min(),
                    BuyCount = buySignals.Length,
                    MaximumHoldingPeriod = GetMaximumHoldPeriod(history, buySignals),
                    SharpeRatio = CalculateSharpeRatio(history, currentMarketWorth),
                    MarketCorrelation = CalculateCorrelation(history),
                    Accuracy = CalculateAccuracy(confusionMatrix, history),
                    Recall = CalculateRecall(confusionMatrix),
                    Precision = CalculatePrecision(confusionMatrix),
                    ConfusionMatrix = confusionMatrix,
                    AverageReturn = GetAverageReturn(buySignals),
                    
                    History = history
                        .Select(x => x.Worth)
                        .ToArray(),

                    StrategyType = strategy.StrategyType.GetDescription()
                });
            }
        }

        public async Task SaveChart(ResultsChart chartType, string path)
            => await _charts[chartType].Save(path);

        public Dictionary<Investor, IEnumerable<SimulationResult>> GetResults()
            => _results.GroupBy(k => k.Investor)
                .ToDictionary(k => k.Key, v => v.AsEnumerable());

        public async Task SaveSimulationResults()
            => await _simulationResultsRepository.Save(_results);

        public async Task SaveData(IEnumerable<MarketData> data)
            => await _marketDataRepository.Save(data);

        public static bool ShouldBuy(IEnumerable<SimulationResult> results)
            => results.Count(x => x.ShouldBuy) > 1;

        public static decimal TotalProfit(IEnumerable<SimulationResult> results)
            => results.Average(x => x.ProfitTotal);

        public static decimal Stake(IEnumerable<SimulationResult> results)
            => ShouldBuy(results)
                ? results.Where(x => x.ShouldBuy).Average(x => x.Stake)
                : 0m;

        public static decimal TotalFunds(IEnumerable<SimulationResult> results)
            => ShouldBuy(results)
                ? results.Where(x => x.ShouldBuy).Average(x => x.TotalFunds)
                : results.Average(x => x.TotalFunds);

        public static decimal StakePercent(IEnumerable<SimulationResult> results)
        {
            var totalFunds = TotalFunds(results);
            if (totalFunds == 0) return 1;
            
            return Stake(results) / totalFunds;
        }

        public decimal MarketAverage()
            => _ratingService
                .GetMarketAverageWorth()
                .Skip(_marketDataCache.BacktestingIndex)
                .Last();

        public decimal CurrentPrice() => _marketDataCache[_marketDataCache.Count - 1].Price;

        public float PriceProbability(Period period) => _marketStatisticsProvider.Probability(period, CurrentPrice(), CurrentPrice());
        public (decimal Price, float Probability) MaximumPriceProbability(Period period) => _marketStatisticsProvider.MaximumProbability(CurrentPrice(), period);

        private void AddRelativeChartSeries(SimulationState[] history, IStrategy strategy)
        {
            var marketAverage = _ratingService
                .GetMarketAverageWorth()
                .Skip(_marketDataCache.BacktestingIndex)
                .ToArray();

            var initialValue = marketAverage.First();
            var relative = marketAverage
                .Select((x, i) => (double) (x - (history[i].Worth + initialValue)));

            _charts[ResultsChart.Relative].AddSeries(relative, strategy.StrategyType.GetDescription());
        }

        private void AddSignalsChartSeries(SimulationState[] history, IStrategy strategy)
        {
            var signals = history
                .Select((x, i) => x.ShouldBuy ? (double)_ratingService.GetMarketAverageWorth(i) : 0d)
                .ToArray();

            _charts[ResultsChart.Signal].AddSeries(signals, strategy.StrategyType.GetDescription(), Chart.Type.Point);
        }

        private void AddPerformanceChartSeries(SimulationState[] history, IStrategy strategy)
        {
            var chartData = history.Select(t => (double)t.Worth);
            _charts[ResultsChart.Performance].AddSeries(chartData, strategy.StrategyType.GetDescription());
        }

        private static IOrderedEnumerable<KeyValuePair<IStrategy, SimulationState[]>> OrderResults(
            ConcurrentDictionary<IStrategy, SimulationState[]> source)
            => source.OrderByDescending(x => x.Value.Last().Worth);

        private static int GetMaximumHoldPeriod(SimulationState[] history, SimulationState[] buySignals)
        {
            if (!buySignals.Any())
                return history.Length;

            var buyIndexes = buySignals.Select(x => Array.IndexOf(history, x)).ToArray();

            return buyIndexes.Skip(1).Select((x, i) => x > 0 ? x - buyIndexes[i] : x).Max();
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
            var observed = history.Select(x => (double)x.Worth).ToArray();
            var modeled = _ratingService.GetMarketAverageWorth().Select(x => (double)x);

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
                ? x.Worth - _ratingService.GetMarketAverageWorth(i)
                : x.Worth).ToArray();
        }

        private static decimal CalculatePrecision(Dictionary<ConfusionCategory, int> confusionMatrix)
        {
            var denominator = confusionMatrix[ConfusionCategory.TruePositive] + confusionMatrix[ConfusionCategory.FalsePositive];

            return denominator != 0 ? (decimal) confusionMatrix[ConfusionCategory.TruePositive] / denominator : 0;
        }

        private static decimal CalculateRecall(Dictionary<ConfusionCategory, int> confusionMatrix)
        {
            var denominator = confusionMatrix[ConfusionCategory.TruePositive] + confusionMatrix[ConfusionCategory.FalseNegative];

            return denominator != 0 ? (decimal) confusionMatrix[ConfusionCategory.TruePositive] / denominator : 0;
        }

        private static decimal CalculateAccuracy(Dictionary<ConfusionCategory, int> confusionMatrix, IList<SimulationState> history)
            => (decimal) (confusionMatrix[ConfusionCategory.TruePositive] + confusionMatrix[ConfusionCategory.TrueNegative]) / history.Count;

        private Dictionary<ConfusionCategory, int> CalculateConfusionMatrix(IList<SimulationState> history)
        {
            var confusionMatrix = new Dictionary<ConfusionCategory, int>
            {
                { ConfusionCategory.TruePositive, 0 },
                { ConfusionCategory.FalsePositive, 0 },
                { ConfusionCategory.TrueNegative, 0 },
                { ConfusionCategory.FalseNegative, 0 }
            };

            var optimalBuys = _ratingService
                .GetMarketMaximum()
                .Where(x => x.ShouldBuy)
                .Select(x => x.Date).ToArray();
            var strategyBuys = history.Where(x => x.ShouldBuy).Select(x => x.Date).ToArray();
            foreach (var h in history)
            {
                var ideal = optimalBuys.Contains(h.Date);
                var model = strategyBuys.Contains(h.Date);

                if (model && ideal)
                    confusionMatrix[ConfusionCategory.TruePositive]++;
                if (!model && !ideal)
                    confusionMatrix[ConfusionCategory.TrueNegative]++;
                if (model && !ideal)
                    confusionMatrix[ConfusionCategory.FalsePositive]++;
                if (!model && ideal)
                    confusionMatrix[ConfusionCategory.FalseNegative]++;
            }

            return confusionMatrix;
        }

        private static decimal CalculateAlpha(decimal currentWorth, decimal currentMarketWorth)
        {
            var excessReturn = currentWorth - currentMarketWorth;
            return excessReturn / currentMarketWorth;
        }

        private static decimal CalculateYTDProfit(Investor investor, IList<SimulationState> history)
        {
            var latestState = history.Last();
            var yearOpenDay = history.ToList().FindIndex(x => x.Date > new DateTime(latestState.Date.Year, 1, 1));
            var strategyOpenWorth = history[yearOpenDay].Worth;
            var investment = GetInvestmentSince(investor.DailyFunds, history.Count - yearOpenDay);
            return latestState.Worth - strategyOpenWorth - investment;
        }

        private static decimal GetInvestmentSince(decimal dailyFunds, int simulationDays)
            => simulationDays * dailyFunds;
    }
}
