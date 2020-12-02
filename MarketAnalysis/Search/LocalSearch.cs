using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Search
{
    public class LocalSearch : ISearcher
    {
        private readonly ISimulator _simulator;
        private readonly IProgressBar _progress;
        private readonly IStrategy _initial;
        private readonly IStrategy[] _potentials;

        private int _margin = 10;
        private Dictionary<IStrategy, decimal> _lookup = new Dictionary<IStrategy, decimal>();

        public LocalSearch(ISimulator simulator, IStrategy initial, IEnumerable<IStrategy> potentials, IProgressBar progress)
        {
            _progress = progress;
            _simulator = simulator;
            _initial = initial;
            _potentials = potentials.ToArray();
        }

        public IStrategy Maximum(DateTime endDate)
        {
            var maximumValue = 0m;
            IStrategy maximumStrategy = _initial;

            var index = Array.IndexOf(_potentials, maximumStrategy);
            var start = Math.Max(0, index - _margin);
            var count = Math.Min(_potentials.Length - 1, (start + _margin));

            var potentials = new List<(IStrategy strategy, decimal value)>();
            for (int i = start; i < count; i++)
            {
                var strategy = _potentials[i];
                if (!_lookup.TryGetValue(strategy, out var value))
                {
                    value = _simulator.Evaluate(strategy, endDate).Last().Worth;
                    _lookup[strategy] = value;
                }
                _progress?.Tick();
                potentials.Add((strategy, value));
            }

            var optimal = potentials.OrderByDescending(x => x.value).First();
            maximumValue = optimal.value;
            maximumStrategy = optimal.strategy;

            ClearCache(potentials.Select(x => x.strategy), optimal.strategy);

            return maximumStrategy;
        }

        private void ClearCache(IEnumerable<IStrategy> potentials, IStrategy optimal)
        {
            _simulator.RemoveCache(potentials.Except(new[] { optimal }));
        }
    }
}
