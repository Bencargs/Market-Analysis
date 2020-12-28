using MarketAnalysis.Strategy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public interface ISimulationCache
    {
        int Count { get; }
        bool GetOrCreate((IStrategy strategy, DateTime day) key, Func<bool> createItem);
        void Remove(DateTime fromDate, DateTime toDate, IEnumerable<IStrategy> strategies);
    }

    public sealed class SimulationCache : ISimulationCache
    {
        private readonly ConcurrentDictionary<IStrategy, List<(DateTime Date, bool ShouldBuy)>> _cache = new();
        private readonly TupleDateComparer _comparer = new();

        public int Count => _cache.Values.Count;
        
        public bool GetOrCreate((IStrategy strategy, DateTime day) key, Func<bool> createItem)
        {
            if (!_cache.TryGetValue(key.strategy, out var history))
            {
                history = new List<(DateTime Date, bool ShouldBuy)>();
                _cache.TryAdd(key.strategy, history);
            }

            var latestState = history.LastOrDefault();
            if (FindEntry(history, latestState, key.day, out var cacheEntry))
                return cacheEntry;

            var entry = CreateEntry(history, key.day, createItem);
            return entry.Item2;
        }

        public void Remove(DateTime fromDate, DateTime toDate, IEnumerable<IStrategy> strategies)
        {
            foreach (var strategy in strategies)
            {
                if (!_cache.TryGetValue(strategy, out var items))
                    continue;

                foreach (var item in items.ToArray())
                {
                    if (item.Date < fromDate)
                        continue;
                    if (item.Date <= toDate)
                        items.Remove(item);
                    else break;
                }
            }
        }
        
        private static (DateTime, bool) CreateEntry(ICollection<(DateTime, bool)> history, DateTime date, Func<bool> createItem)
        {
            (DateTime, bool) cacheEntry = new();
            if (createItem == null) 
                return cacheEntry;
            
            cacheEntry = (date, createItem());
            history.Add(cacheEntry);
            return cacheEntry;
        }
        private bool FindEntry(List<(DateTime Date, bool ShouldBuy)> history, (DateTime Date, bool ShouldBuy) latestState, DateTime date, out bool shouldBuy)
        {
            shouldBuy = false;
            if (date > latestState.Date) 
                return false;
            
            var index = history.BinarySearch((date, false), _comparer);
            if (index <= -1) 
                return false;
            
            shouldBuy = history[index].ShouldBuy;
            return true;
        }
        
        private class TupleDateComparer : IComparer<(DateTime, bool)>
        {
            public int Compare((DateTime, bool) x, (DateTime, bool) y)
            {
                return x.Item1.CompareTo(y.Item1);
            }
        }
    }
}
