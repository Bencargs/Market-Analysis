using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Caching
{
    public sealed class SimulationCache
    {
        private readonly Dictionary<IStrategy, List<SimulationState>> _cache;

        public SimulationCache()
        {
            _cache = new Dictionary<IStrategy, List<SimulationState>>();
        }

        public SimulationState GetOrCreate((IStrategy strategy, DateTime day) key, Func<SimulationState, SimulationState> createItem)
        {
            if (!_cache.TryGetValue(key.strategy, out var history))
            {
                history = new List<SimulationState>(5000);
                _cache.Add(key.strategy, history);
            }

            var latestState = history.LastOrDefault();
            if (key.day <= latestState?.Date)
            {
                var index = history.BinarySearch(new SimulationState { Date = key.day }, new SimulationStateDateComparer());
                if (index > -1)
                    return history[index];
            }

            var previousState = latestState ?? new SimulationState();
            var cacheEntry = createItem(previousState);
            history.Add(cacheEntry);

            return cacheEntry;
        }

        private class SimulationStateDateComparer : IComparer<SimulationState>
        {
            public int Compare(SimulationState x, SimulationState y)
            {
                return x.Date.CompareTo(y.Date);
            }
        }
    }
}
