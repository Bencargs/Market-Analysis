using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace MarketAnalysis
{
    public sealed class SimulationCache : IDisposable
    {
        private static readonly Lazy<SimulationCache> _instance = new Lazy<SimulationCache>(() => new SimulationCache());
        public static SimulationCache Instance => _instance.Value;
        private MemoryCache _cache;

        private SimulationCache()
        {
            _cache = InitializeCache();
        }

        public bool IsEnabled { get; set; }

        public SimulationState GetOrCreate((IStrategy strategy, int day) key, Func<SimulationState> createItem)
        {
            if (!IsEnabled)
                return createItem();

            if (!_cache.TryGetValue(key, out SimulationState cacheEntry))
            {
                cacheEntry = createItem();

                _cache.Set(key, cacheEntry, options: new MemoryCacheEntryOptions { Size = 1 });
            }
            return cacheEntry;
        }

        private MemoryCache InitializeCache()
        {
            return new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = Configuration.CacheSize,
                CompactionPercentage = 0.8
            });
        }

        public void Dispose()
        {
            _cache?.Dispose();
            _cache = InitializeCache();
        }
    }
}
