using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Simulation;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : OptimisableStrategy
    {
        private int _threshold;
        private decimal _previousVolume;
        public override StrategyType StrategyType { get; } = StrategyType.Volume;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(256);

        public VolumeStrategy()
            : this (0)
        { }

        public VolumeStrategy(int threshold, bool shouldOptimise = true)
            :base(shouldOptimise)
        {
            _threshold = threshold;
        }

        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
        {
            var potentials = Enumerable.Range(1, 800).Select(x => new VolumeStrategy(x, false));

            var searcher = new LinearSearch(simulator, potentials, progress);
            simulator.RemoveCache(potentials.Except(new[] { this }));
            return searcher.Maximum(LatestDate);
        }

        protected override void SetParameters(IStrategy strategy)
        {
            _threshold = ((VolumeStrategy)strategy)._threshold;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var shouldBuy = _previousVolume != 0 && 
                (data.Volume / _previousVolume) > _threshold;
            
            _previousVolume = data.Volume;
            return shouldBuy;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is VolumeStrategy strategy))
                return false;

            return _threshold == strategy._threshold;
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode();
        }
    }
}
