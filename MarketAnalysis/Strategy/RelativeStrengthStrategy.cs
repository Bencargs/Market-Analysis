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
        private int[] _testSet;
        public override StrategyType StrategyType { get; } = StrategyType.RelativeStrength;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(256);

        private static readonly Lazy<List<int[]>> OptimisationSets = new Lazy<List<int[]>>(() =>
        {
            var results = new List<int[]>();

            var minSetSize = 4;
            var maxPercentile = 10;
            foreach (var s in Enumerable.Range(minSetSize, maxPercentile))
            {
                var combinations = GetCombinations(s, maxPercentile);
                foreach (var sets in combinations)
                {
                    results.Add(sets.Select(x => x - 1).ToArray());
                }
            }
            return results;
        });

        public RelativeStrengthStrategy(int threshold, int[] testSet, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _testSet = testSet;
            _threshold = threshold;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return new[] { 30, 35, 40, 45, 50, 55, 60 }.SelectMany(lookback =>
            {
                return OptimisationSets.Value.Select(s => 
                    new RelativeStrengthStrategy(lookback, s, false));
            });
        }

        public override void SetParameters(IStrategy strategy)
        {
            var optimal = (RelativeStrengthStrategy)strategy;

            _threshold = optimal._threshold;
            _testSet = optimal._testSet;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var batch = MarketDataCache.Instance.GetLastSince(LatestDate, _threshold).ToArray();
            if (batch.Count() < 3)
                return false;

            var strength = GetRelativeStrength(data.Price, batch);

            return _testSet.Contains(strength);
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

        private static IEnumerable<int[]> GetCombinations(int setSize, int numbers)
        {
            var result = new int[setSize];
            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                var index = stack.Count - 1;
                var value = stack.Pop();

                while (value <= numbers)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index == setSize)
                    {
                        yield return result;
                        break;
                    }
                }
            }
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
