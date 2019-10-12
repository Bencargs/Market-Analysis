using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace MarketAnalysis
{
    public static class SimulationCache
    {
        private static MemoryCache _cache = InitializeCache();

        public static SimulationState GetOrCreate(Tuple<IStrategy, int> key, Func<SimulationState> createItem)
        {
            if (!_cache.TryGetValue(key, out SimulationState cacheEntry))
            {
                cacheEntry = createItem();

                _cache.Set(key, cacheEntry, options: new MemoryCacheEntryOptions { Size = 1 });
            }
            return cacheEntry;
        }

        public static void ClearCache()
        {
            _cache?.Dispose();
            _cache = InitializeCache();
        }

        private static MemoryCache InitializeCache()
        {
            return new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = Configuration.CacheSize,
                CompactionPercentage = 0.8
            });
        }
    }
}
