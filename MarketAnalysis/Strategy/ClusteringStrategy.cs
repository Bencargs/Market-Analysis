using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Range = MarketAnalysis.Models.Range;

namespace MarketAnalysis.Strategy
{
    public class ClusteringStrategy : OptimisableStrategy
    {
        public override StrategyType StrategyType => StrategyType.Cluster;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(1024);
        private (Range Container, Func<MarketData, decimal> Selector)[] _features;

        public ClusteringStrategy(
            (Range, Func<MarketData, decimal>)[] features, 
            bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _features = features;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            var dataset = MarketDataCache.Instance.TakeUntil(LatestDate);
            var feature1 = dataset.Select(d => _features[0].Selector(d));
            var feature2 = dataset.Select(d => _features[1].Selector(d));

            return Enumerable.Range(10, 5).SelectMany(size =>
            {
                return Partition(feature1, size).SelectMany(p1 =>
                {
                    return Partition(feature2, size).Select(p2 =>
                    {
                        var parameters = new[]
                        {
                            (p1, _features[0].Selector),
                            (p2, _features[1].Selector),
                        };
                        return new ClusteringStrategy(parameters, false);
                    });
                });
            });
        }

        public override void SetParameters(IStrategy strategy)
        {
            _features = ((ClusteringStrategy)strategy)._features;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            foreach (var (Container, Selector) in _features)
            {
                var feature = Selector(data);
                if (!Container.Contains(feature))
                    return false;
            }
            return true;
        }

        private static Range[] Partition(IEnumerable<decimal> source, int partitions)
        {
            var orderedSource = source.OrderBy(x => x).ToArray();
            var population = orderedSource.Length;
            var partitionSize = (int)Math.Ceiling((double)population / partitions);

            var bucket = new List<Range>(partitions);
            foreach (var batch in orderedSource.Batch(partitionSize))
            {
                bucket.Add(new Range(batch));
            }
            return bucket.Distinct().ToArray();
        }
    }
}
