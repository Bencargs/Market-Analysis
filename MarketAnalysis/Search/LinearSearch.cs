using MarketAnalysis;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Search
{
    public class LinearSearch : ISearcher
    {
        private readonly ISimulator _simulator;
        private readonly IProgressBar _progress;
        private readonly IEnumerable<IStrategy> _potentials;

        public LinearSearch(ISimulator simulator, IEnumerable<IStrategy> potentials, IProgressBar progress)
        {
            _progress = progress;
            _simulator = simulator;
            _potentials = potentials;
        }

        public IStrategy Maximum(DateTime endDate)
        {
            _progress.MaxTicks = _potentials.Count();
            var potentials = _potentials.Select(strat =>
            {
                var result = _simulator.Evaluate(strat, endDate).Last();
                _progress?.Tick();
                return (result.Worth, result.BuyCount, strat);
            })
            .OrderByDescending(x => x.Worth)
            .ThenBy(x => x.BuyCount)
            .Select(x => x.strat);
            
            var optimal = potentials.First();
            ClearCache(potentials, optimal);

            return optimal;
        }

        private void ClearCache(IEnumerable<IStrategy> potentials, IStrategy optimal)
        {
            _simulator.RemoveCache(potentials.Except(new[] { optimal }));
        }
    }
}
