using MarketAnalysis.Models;
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
        bool TryGet((IStrategy strategy, DateTime day) key, out SimulationState simulation);
        SimulationState GetOrCreate((IStrategy strategy, DateTime day) key, Func<SimulationState, SimulationState> createItem);
        void Remove(DateTime fromDate, DateTime toDate, IEnumerable<IStrategy> strategies);
    }

    public sealed class SimulationCache : ISimulationCache
    {
        private readonly ConcurrentDictionary<IStrategy, List<SimulationState>> _cache = new ConcurrentDictionary<IStrategy, List<SimulationState>>();
        private readonly SimulationStateDateComparer _comparer = new SimulationStateDateComparer();

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
                _cache.TryAdd(key.strategy, history);
            }

            var latestState = history.LastOrDefault();
            if (FindEntry(history, latestState, key.day, out var cacheEntry))
                return cacheEntry;

            return CreateEntry(history, latestState, createItem);
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

        private static SimulationState CreateEntry(List<SimulationState> history, SimulationState latestState, Func<SimulationState, SimulationState> createItem)
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
