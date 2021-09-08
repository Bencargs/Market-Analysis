using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Staking
{
    public class ProbabilityKellyStaking : IStakingService
    {
        private readonly ProbabilityStrategy _strategy;
        private readonly IMarketDataCache _marketDataCache;
        private decimal _fraction = 1;

        public ProbabilityKellyStaking(
            ProbabilityStrategy strategy,
            IMarketDataCache marketDataCache)
        {
            _strategy = strategy;
            _marketDataCache = marketDataCache;
        }

        public void Evaluate(DateTime fromDate, DateTime toDate)
        {
            var marketData = _marketDataCache.TakeFrom(fromDate, toDate).ToArray();

            var histogram = ((ProbabilityParameters)_strategy.Parameters).Histogram;
            var currentPrice = Convert.ToInt32(marketData.Last().DeltaPercent);

            var tradeValue = (decimal)histogram[currentPrice].Average();

            var total = (decimal)histogram.Select(x => x.Value).Count();
            var probability = histogram[currentPrice].Count / total;

            var fraction = probability - (1 - probability) / tradeValue;

            _fraction = fraction.Clamp(0, 1);
        }

        public decimal GetStake(decimal totalFunds)
        {
            return totalFunds * _fraction;
        }
    }
}
