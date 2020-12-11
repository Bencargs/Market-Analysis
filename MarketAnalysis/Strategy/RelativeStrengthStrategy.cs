using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : IStrategy
    {
        private readonly StrategyFactory _strategyFactory;
        private readonly MarketDataCache _marketDataCache;
        private readonly ISearcher _searcher;
        private RelativeStrengthParameters _parameters;

        public IParameters Parameters 
        {
            get => _parameters;
            private set => _parameters = (RelativeStrengthParameters)value; 
        }
        public StrategyType StrategyType { get; } = StrategyType.RelativeStrength;
        

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

        public RelativeStrengthStrategy(
            StrategyFactory strategyFactory,
            MarketDataCache marketDataCache,
            ISearcher searcher,
            RelativeStrengthParameters parameters)
        {
            _searcher = searcher;
            _strategyFactory = strategyFactory;
            _marketDataCache = marketDataCache;

            Parameters = parameters;
        }

        public void Optimise(DateTime latestDate)
        {
            var potentials = new[] { 30, 35, 40, 45, 50, 55, 60 }.SelectMany(t =>
            {
                return OptimisationSets.Value.Select(s =>
                    _strategyFactory.Create(new RelativeStrengthParameters { Threshold = t, TestSet = s }));
            });

            var optimum = _searcher.Maximum(potentials, latestDate);

            Parameters = optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.GetLastSince(data.Date, _parameters.Threshold).ToArray();
            if (batch.Count() < 3)
                return false;

            var strength = GetRelativeStrength(data.Price, batch);

            return _parameters.TestSet.Contains(strength);
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

            return strategy._parameters.Threshold == _parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return _parameters.Threshold.GetHashCode();
        }
    }
}
