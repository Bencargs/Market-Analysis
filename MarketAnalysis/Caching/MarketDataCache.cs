using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public sealed class MarketDataCache : IDisposable
    {
        private static readonly Lazy<MarketDataCache> _instance = new Lazy<MarketDataCache>(() => new MarketDataCache());
        private readonly SortedDictionary<DateTime, MarketData> _cache = new SortedDictionary<DateTime, MarketData>(new Dictionary<DateTime, MarketData>(5000));

        public static MarketDataCache Instance => _instance.Value;
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

        public IEnumerable<MarketData> TakeUntil(DateTime? date = null)
        {
            var endDate = date ?? DateTime.MaxValue;
            foreach (var (dateKey, row) in _cache)
            {
                if (dateKey <= endDate)
                {
                    yield return row;
                }
            }
        }

        public void Initialise(IEnumerable<MarketData> data)
        {
            _cache.Clear();
            foreach (var d in data)
                _cache.Add(d.Date, d);
        }
        
        public void Dispose()
        {
            _cache.Clear();
        }
    }
}
