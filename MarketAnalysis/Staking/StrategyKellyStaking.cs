using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Strategy;

namespace MarketAnalysis.Staking
{
    public class StrategyKellyStaking : IStakingService
    {
        private readonly IStrategy _strategy;
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISimulationCache _simulationCache;
        private decimal _fraction = 1;

        public StrategyKellyStaking(
            IStrategy strategy,
            IMarketDataCache marketDataCache,
            ISimulationCache simulationCache)
        {
            _strategy = strategy;
            _marketDataCache = marketDataCache;
            _simulationCache = simulationCache;
        }

        public void Evaluate(DateTime fromDate, DateTime toDate)
        {
            var marketData = _marketDataCache.TakeFrom(fromDate, toDate)
                .Select(x => x.DeltaPercent / 100)
                .Where(_ =>
                {
                    var found = _simulationCache.TryGet((_strategy, toDate), out var shouldBuy);
                    if (!found)
                        return false;

                    return shouldBuy;
                })
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


            var fraction = probability - (1 - probability) / tradeValue;

            _fraction = fraction.Clamp(0, 1);
        }

        public decimal GetStake(DateTime _, decimal totalFunds)
        {
            return totalFunds * _fraction;
        }
    }
}
