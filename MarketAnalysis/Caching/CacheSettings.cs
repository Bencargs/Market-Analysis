using Microsoft.Extensions.Caching.Memory;
using System;

namespace MarketAnalysis.Caching
{
    public class CacheSettings : IDisposable
    {
        public bool IsEnabled { get; set; } = true;
        public double CompactionPercentage { get; set; } = 0.8;
        public long CacheSize { get; set; } = Configuration.CacheSize;
        public MemoryCacheEntryOptions EntryOptions { get; set; }
            = new MemoryCacheEntryOptions { Size = 1 };

        private readonly CacheSettings _previousSettings;

        public CacheSettings()
        {
            _previousSettings = SimulationCache.Settings;
            SimulationCache.Settings = this;
        }

        public void Dispose()
        {
            SimulationCache.Settings = _previousSettings;
        }
    }
}
