using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public sealed class MarketDataCache : IDisposable
    {
        private static readonly Lazy<MarketDataCache> _instance = new Lazy<MarketDataCache>(() => new MarketDataCache());
        public static MarketDataCache Instance => _instance.Value;
        private SortedDictionary<DateTime, MarketData> _cache = new SortedDictionary<DateTime, MarketData>(new Dictionary<DateTime, MarketData>(5000));

        public int Count => _cache.Count;

        public IEnumerable<MarketData> GetLastSince(DateTime date, int count)
        {
            var results = new List<MarketData>();
            var remaining = count;
            foreach (var (dateKey, row) in _cache.Reverse())
            {
                if (remaining <= 0)
                    break;

                if (dateKey <= date)
                {
                    results.Add(row);
                    remaining--;
                }
            }
            return results;
        }

        public IEnumerable<MarketData> TakeUntil(DateTime date)
        {
            foreach (var (dateKey, row) in _cache)
            {
                if (dateKey <= date)
                {
                    yield return row;
                }
            }
        }

        public bool TryAdd(MarketData data)
        {
            if (_cache.ContainsKey(data.Date))
                return false;

            _cache.Add(data.Date, data);
            return true;
        }

        public void Dispose()
        {
            _cache.Clear();
        }
    }
}
