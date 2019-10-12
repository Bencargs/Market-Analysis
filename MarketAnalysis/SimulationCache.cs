using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace MarketAnalysis
{
    public static class SimulationCache
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        public static int Hit;
        public static int Miss;

        public static SimulationState GetOrAdd(Tuple<IStrategy, int> key, Func<SimulationState> createItem)
        {
            if (!_cache.TryGetValue(key, out SimulationState cacheEntry))
            {
                cacheEntry = createItem();

                _cache.Set(key, cacheEntry);
                Miss++;
            }
            else { Hit++; }
            return cacheEntry;
        }
    }
}
