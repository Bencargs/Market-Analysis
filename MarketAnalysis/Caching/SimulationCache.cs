using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace MarketAnalysis.Caching
{
    public sealed class SimulationCache : IDisposable
    {
        private static readonly Lazy<SimulationCache> _instance = new Lazy<SimulationCache>(() => new SimulationCache());
        public static SimulationCache Instance => _instance.Value;
        public static CacheSettings Settings { get; set; }
        private MemoryCache _cache;

        private SimulationCache()
        {
            Settings = new CacheSettings();
            _cache = InitializeCache();
        }
        
        public SimulationState GetOrCreate((IStrategy strategy, int day) key, Func<SimulationState> createItem)
        {
            if (!Settings.IsEnabled)
                return createItem();

            if (!_cache.TryGetValue(key, out SimulationState cacheEntry))
            {
                cacheEntry = createItem();

                _cache.Set(key, cacheEntry, options: Settings.EntryOptions);
            }
            return cacheEntry;
        }

        private MemoryCache InitializeCache()
        {
            return new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = Settings.CacheSize,
                CompactionPercentage = Settings.CompactionPercentage
            });
        }

        public void Dispose()
        {
            _cache?.Dispose();
            _cache = InitializeCache();
        }
    }
}
