using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public interface IMarketDataCache
    {
        int Count { get; }
        int BacktestingIndex { get; }
        void Initialise(IEnumerable<MarketData> data);
        MarketData this[int index] { get; }
        IEnumerable<MarketData> GetLastSince(DateTime date, int count);
        IEnumerable<MarketData> TakeUntil(DateTime? date = null);
        IEnumerable<MarketData> TakeFrom(DateTime fromDate, DateTime? endDate);
    }

    public sealed class MarketDataCache : IMarketDataCache
    {
        private MarketData[] _cache;

        public int Count => _cache.Length;
        public int BacktestingIndex { get; private set; }

        public void Initialise(IEnumerable<MarketData> data)
        {
            _cache = data.OrderBy(x => x.Date).ToArray();
            BacktestingIndex = _cache.TakeWhile(x => x.Date < Configuration.BacktestingDate).Count() + 1;
        }

        public MarketData this[int index] => _cache[index];

        public IEnumerable<MarketData> GetLastSince(DateTime date, int count)
        {
            var remaining = count;
            for (int i = _cache.Length; i --> 0;)
            {
                var data = _cache[i];
                if (remaining <= 0)
                    break;

                if (data.Date <= date)
                {
                    remaining--;
                    yield return data;
                }
            }
        }

        public IEnumerable<MarketData> TakeUntil(DateTime? date = null)
        {
            var endDate = date ?? DateTime.MaxValue;
            foreach (var data in _cache)
            {
                if (data.Date <= endDate)
                    yield return data;
                else break;
            }
        }

        public IEnumerable<MarketData> TakeFrom(DateTime fromDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.MaxValue;
            foreach (var data in _cache)
            {
                if (data.Date <= fromDate)
                    continue;
                if (data.Date <= end)
                    yield return data;
                else break;
            }
        }
    }
}
