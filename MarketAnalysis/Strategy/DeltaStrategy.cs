using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Simulation;
using ShellProgressBar;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : OptimisableStrategy
    {
        private decimal _threshold;
        public override StrategyType StrategyType { get; } = StrategyType.Delta;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(128);

        public DeltaStrategy()
            : this (0)
        { }

        public DeltaStrategy(decimal threshold, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _threshold = threshold;
        }

        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
        {
            var potentials = Enumerable.Range(1, 100).Select(x =>
            {
                var parameter = (decimal)x / 1000;
                return new DeltaStrategy(parameter, false);
            });

            var searcher = new LinearSearch(simulator, potentials, progress);
            simulator.RemoveCache(potentials.Except(new[] { this }));
            return searcher.Maximum(LatestDate);
        }

        protected override void SetParameters(IStrategy strategy)
        {
            _threshold = ((DeltaStrategy)strategy)._threshold;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            return data.Delta < _threshold;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DeltaStrategy strategy))
                return false;

            return strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode();
        }
    }
}
