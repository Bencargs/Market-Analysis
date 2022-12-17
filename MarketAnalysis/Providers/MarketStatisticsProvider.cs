using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Providers
{
    public class MarketStatisticsProvider
    {
        private readonly IMarketDataCache _marketDataCache;

        private readonly Dictionary<float, int> _dailyHistogram = new();
        private readonly Dictionary<float, int> _weeklyHistogram = new();
        private readonly Dictionary<float, int> _monthlyHistogram = new();
        private readonly Dictionary<float, int> _quarterlyHistogram = new();

        public MarketStatisticsProvider(IMarketDataCache marketDataCache) => _marketDataCache = marketDataCache;

        public void Initialise()
        {
            bool InRange(int i) => i < (_marketDataCache.Count - _marketDataCache.BacktestingIndex);

            for (var i = _marketDataCache.BacktestingIndex; InRange(i); i++) 
            {
                float GetFuturePercent(Period futureDays) => GetPercent(_marketDataCache[i].Price, _marketDataCache[i + (int)futureDays].Price);

                Dictionary<float, int> TryGetHistogram(Period period) => InRange(i - (int)period) ? GetHistogram(period) : null;

                TryGetHistogram(Period.Day).Increment(GetFuturePercent(Period.Day));

                TryGetHistogram(Period.Week).Increment(GetFuturePercent(Period.Week));

                TryGetHistogram(Period.Month).Increment(GetFuturePercent(Period.Month));

                TryGetHistogram(Period.Quarter).Increment(GetFuturePercent(Period.Quarter));
            }
        }

        public float Probability(
            Period period,
            decimal currentPrice,
            decimal desiredPrice)
        {
            var percent = GetPercent(currentPrice, desiredPrice);

            var histogram = GetHistogram(period);

            return histogram.Average(percent);
        }

        public (decimal Price, float Probability) MaximumProbability(
            decimal currentPrice)
        {
            var bucket = GetHistogram(Period.Day); // Any period returns the same probability

            var maximum = Enumerable.Range(-10, 200)
                .Select(x => currentPrice + ((decimal)x / 100))
                .Select(price =>
                {
                    var percent = GetPercent(currentPrice, price);
                    return (price, bucket.Average(percent));
                })
                .OrderByDescending(x => x.Item2)
                .First();
            return maximum;
        }

        private Dictionary<float, int> GetHistogram(Period period) =>
            period switch
            {
                Period.Day => _dailyHistogram,
                Period.Week => _weeklyHistogram,
                Period.Month => _monthlyHistogram,
                Period.Quarter => _quarterlyHistogram,
                _ => throw new ArgumentException($"Period {period} is unsupported")
            };

        private static float GetPercent(decimal current, decimal future)
        {
            var delta = (future - current) / current;
            var percent = (float)Math.Round(delta, 4, MidpointRounding.AwayFromZero);
            return percent;
        }
    }
}
