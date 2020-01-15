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
        private readonly SimulationStateDateComparer _comparer = new SimulationStateDateComparer();

        public SimulationCache()
        {
            _cache = new Dictionary<IStrategy, List<SimulationState>>();
        }

        public int Count => _cache.Values.Count;

        public bool TryGet((IStrategy strategy, DateTime day) key, out SimulationState simulation)
        {
            simulation = null;
            if (!_cache.TryGetValue(key.strategy, out var history))
                return false;

            var latestState = history.LastOrDefault();
            if (FindEntry(history, latestState, key.day, out var cacheEntry))
                simulation = cacheEntry;

            return simulation != null;
        }

        public SimulationState GetOrCreate((IStrategy strategy, DateTime day) key, Func<SimulationState, SimulationState> createItem)
        {
            if (!_cache.TryGetValue(key.strategy, out var history))
            {
                history = new List<SimulationState>();
                _cache.Add(key.strategy, history);
            }

            var latestState = history.LastOrDefault();
            if (FindEntry(history, latestState, key.day, out var cacheEntry))
                return cacheEntry;

            return CreateEntry(history, latestState, createItem);
        }

        public void Remove(IStrategy strategy)
        {
            _cache.Remove(strategy);
        }

        private SimulationState CreateEntry(List<SimulationState> history, SimulationState latestState, Func<SimulationState, SimulationState> createItem)
        {
            SimulationState cacheEntry = null;
            if (createItem != null)
            {
                var previousState = latestState ?? new SimulationState();
                cacheEntry = createItem(previousState);
                history.Add(cacheEntry);
            }
            return cacheEntry;
        }

        private bool FindEntry(List<SimulationState> history, SimulationState latestState, DateTime date, out SimulationState simulationState)
        {
            simulationState = null;
            if (latestState == null)
                return false;

            if (date <= latestState.Date)
            {
                var index = history.BinarySearch(new SimulationState { Date = date }, _comparer);
                if (index > -1)
                    simulationState = history[index];
            }
            return simulationState != null;
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
