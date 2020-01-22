using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : OptimisableStrategy
    {
        private int _threshold;
        public override StrategyType StrategyType { get; } = StrategyType.RelativeStrength;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(256);

        public RelativeStrengthStrategy(int threshold, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _threshold = threshold;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return Enumerable.Range(1, 101).Select(x =>
                new RelativeStrengthStrategy(x, false));
        }

        public override void SetParameters(IStrategy strategy)
        {
            _threshold = ((RelativeStrengthStrategy)strategy)._threshold;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var batch = MarketDataCache.Instance.GetLastSince(LatestDate, _threshold).ToArray();
            if (batch.Count() < 3)
                return false;

            var strength = GetRelativeStrength(data.Price, batch);

            // todo: these parameters should be derived from self optimisation
            return new[] { 0, 3, 5, 6 }.Contains(strength);
        }

        private int GetRelativeStrength(decimal price, MarketData[] data)
        {
            var min = data.Min(y => y.Price);
            var max = data.Max(y => y.Price);
            var range = max - min;
            if (range == 0)
                return 100;

            var adjustedPrice = price - min;
            return Convert.ToInt32(adjustedPrice / range * 100);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RelativeStrengthStrategy strategy))
                return false;

            return strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode();
        }
    }
}
