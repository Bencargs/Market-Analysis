using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Caching
{
    public sealed class SimulationCache
    {
        public static CacheSettings Settings { get; set; }
        private readonly Dictionary<IStrategy, SortedList<DateTime, SimulationState>> _cache;

        public SimulationCache()
        {
            Settings = new CacheSettings();
            _cache = new Dictionary<IStrategy, SortedList<DateTime, SimulationState>>();
        }
        
        public SimulationState GetOrCreate((IStrategy strategy, DateTime day) key, Func<SimulationState> createItem)
        {
            if (!Settings.IsEnabled)
                return createItem();

            if (!_cache.TryGetValue(key.strategy, out var history))
            {
                history = new SortedList<DateTime, SimulationState>();
                _cache.Add(key.strategy, history);
            }
            if (!history.TryGetValue(key.day, out SimulationState cacheEntry))
            {
                cacheEntry = createItem();
                history.Add(key.day, cacheEntry);
            }

            return cacheEntry;
        }

        public IList<SimulationState> GetHistory(IStrategy strategy)
        {
            _cache.TryGetValue(strategy, out var history);

            return history?.Values ?? new List<SimulationState>();
        }
    }
}
