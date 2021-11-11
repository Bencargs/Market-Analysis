using System;
using System.Linq;
using MarketAnalysis.Caching;

namespace MarketAnalysis.Staking
{
    public class BasicKellyStaking : IStakingService
    {
        private readonly IMarketDataCache _marketDataCache;
        private decimal _fraction = 1;

        public BasicKellyStaking(IMarketDataCache marketDataCache)
        {
            _marketDataCache = marketDataCache;
        }

        public void Evaluate(DateTime fromDate, DateTime toDate)
        {
            var marketData = _marketDataCache.TakeFrom(fromDate, toDate)
                .Select(x => x.DeltaPercent / 100)
                .Split(x => x <= 0);
            var profit = marketData.TrueSet.ToArray();
            var loss = marketData.FalseSet.ToArray();

            if (profit.Length == 0)
            {
                _fraction = 0;
                return;
            }

            if (loss.Length == 0)
            {
                _fraction = 1;
                return;
            }

            var avgProfit = -profit.Average();
            var avgLoss = loss.Average();
            var probability = (decimal)profit.Length / (profit.Length + loss.Length);
            var tradeValue = avgProfit / avgLoss;
            
            var fraction = probability - ((1 - probability) / tradeValue);
            _fraction = fraction.Clamp(0, 1);
        }

        public decimal GetStake(DateTime _, decimal totalFunds)
        {
            return totalFunds * _fraction;
        }
    }
}
