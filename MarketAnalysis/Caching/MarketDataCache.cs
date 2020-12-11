using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public sealed class MarketDataCache
    {
        private MarketData[] _cache;

        public int Count => _cache.Length;
        public int BacktestingIndex => Count - _cache.TakeWhile(x => x.Date < Configuration.BacktestingDate).Count();

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
                if (data.Date < fromDate)
                    continue;
                if (data.Date <= end)
                    yield return data;
                else break;
            }
        }

        public void Initialise(IEnumerable<MarketData> data)
        {
            _cache = data.OrderBy(x => x.Date).ToArray();
        }
    }
}
