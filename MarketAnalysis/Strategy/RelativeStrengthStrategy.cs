using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : IStrategy, IEquatable<RelativeStrengthStrategy>
    {
        private readonly IMarketDataCache _marketDataCache;
        private readonly ISearcher _searcher;
        private RelativeStrengthParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.RelativeStrength;
        
        public RelativeStrengthStrategy(
            IMarketDataCache marketDataCache,
            ISearcher searcher,
            RelativeStrengthParameters parameters)
        {
            _searcher = searcher;
            _marketDataCache = marketDataCache;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = new[] { 30, 35, 40, 45, 50, 55, 60 }.SelectMany(t =>
            {
                return OptimisationSets.Value.Select(s =>
                    new RelativeStrengthParameters { Threshold = t, TestSet = s });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (RelativeStrengthParameters) optimum;
        }

        public bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.GetLastSince(data.Date, _parameters.Threshold).ToArray();
            if (batch.Length < 3)
                return false;

            var strength = GetRelativeStrength(data.Price, batch);

            var testSets = OptimisationSets.Value;
            return testSets.Any(x => x.Contains(strength));
        }

        private static int GetRelativeStrength(decimal price, MarketData[] data)
        {
            var min = data.Min(y => y.Price);
            var max = data.Max(y => y.Price);
            var range = max - min;
            if (range == 0)
                return 100;

            var adjustedPrice = price - min;
            return Convert.ToInt32(adjustedPrice / range * 100);
        }

        private static readonly Lazy<List<int[]>> OptimisationSets = new(() =>
        {
            var results = new List<int[]>();

            var minSetSize = 4;
            var maxPercentile = 8;
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(RelativeStrengthStrategy)) return false;

            return Equals(obj as RelativeStrengthStrategy);
        }

        public bool Equals(RelativeStrengthStrategy obj)
        {
            return obj._parameters.Threshold == _parameters.Threshold
                && obj._parameters.TestSet.SequenceEqual(_parameters.TestSet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold, _parameters.TestSet);
        }
    }
}
