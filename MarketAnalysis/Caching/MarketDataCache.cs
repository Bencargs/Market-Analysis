﻿using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public sealed class MarketDataCache
    {
        private readonly List<MarketData> _cache = new List<MarketData>(5000);

        public int Count => _cache.Count;
        public int BacktestingIndex => Count - _cache.TakeWhile(x => x.Date < Configuration.BacktestingDate).Count();

        public IEnumerable<MarketData> GetLastSince(DateTime date, int count)
        {
            var remaining = count;
            for (int i = _cache.Count; i -- > 0;)
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

        public void Initialise(IEnumerable<MarketData> data)
        {
            foreach (var d in data.OrderBy(x => x.Date))
                _cache.Add(d);
        }
    }
}
