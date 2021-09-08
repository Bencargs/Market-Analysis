using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Services;
using MarketAnalysis.Staking;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class ClusteringStrategy : IStrategy, IEquatable<ClusteringStrategy>
    {

        private readonly Func<MarketData, decimal> _xSelector = x => x.DeltaPercent;
        private readonly Func<MarketData, decimal> _ySelector = x => x.VolumePercent;
        private readonly IMarketDataCache _marketDataCache;
        private readonly RatingService _ratingService;
        private readonly IStakingService _stakingService;
        private readonly ISearcher _searcher;
        private ClusteringParameters _parameters;
        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Cluster;

        public ClusteringStrategy(
            ISearcher searcher,
            IMarketDataCache marketDataCache,
            RatingService ratingService,
            IStakingService stakingService,
            ClusteringParameters parameters)
        {
            _searcher = searcher;
            _marketDataCache = marketDataCache;
            _ratingService = ratingService;
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            _stakingService.Evaluate(fromDate, toDate);

            var history = _marketDataCache.TakeUntil(toDate).ToArray();

            // Build 2d array where each cell is sized to has equal data points and probability
            var xData = history.Select(_xSelector).ToArray();
            var yData = history.Select(_ySelector).ToArray();
            var grid = new Grid<List<decimal>>(xData, yData, _parameters.Partitions);

            // Place each data point in appropriate cell
            var marketDataValues = _ratingService.CalculateMarketDataValues(history);
            foreach (var data in history)
            {
                var x = _xSelector(data);
                var y = _ySelector(data);
                var cell = grid[x, y];
                marketDataValues.TryGetValue(data.Date, out var value);
                cell.Add(value);
            }

            // Determine the average value of each cell
            var averages = grid
                .Where(x => x.Any())
                .Select(GetAverage)
                .Distinct()
                .ToArray();
            var minValue = Convert.ToInt32(Math.Round(averages.Min(), MidpointRounding.ToNegativeInfinity));
            var maxValue = Convert.ToInt32(Math.Round(averages.Max(), MidpointRounding.ToPositiveInfinity));
            var range = maxValue - minValue;

            // Search for optimal buy threshold
            var potentials = Enumerable.Range(minValue, range)
                .SelectMany(t => Enumerable.Range(2, 8)
                    .Select(p => new ClusteringParameters
                    {
                        Partitions = p,
                        Threshold = -t,
                        Grid = grid
                    }));

            var optimal = _searcher.Maximum(potentials, fromDate, toDate);

            _parameters = (ClusteringParameters)optimal;
        }

        public bool ShouldBuy(MarketData data)
        {
            if (!_parameters.Grid.Any())
                return false;

            var x = _xSelector(data);
            var y = _ySelector(data);
            var cell = _parameters.Grid[x, y];
            if (!cell.Any())
                return false;

            var clusterAverage = GetAverage(cell);
            var clusterValue = Convert.ToInt32(Math.Abs(clusterAverage));

            return clusterValue > _parameters.Threshold;
        }

        public decimal GetStake(decimal totalFunds)
            => _stakingService.GetStake(totalFunds);

        private static decimal GetAverage(List<decimal> values)
            => values.Average() * 1_000;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ClusteringStrategy)) return false;

            return Equals(obj as ClusteringStrategy);
        }

        public bool Equals(ClusteringStrategy other)
            => _parameters.Threshold == other._parameters.Threshold &&
               _parameters.Partitions == other._parameters.Partitions;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Threshold, _parameters.Partitions);
    }
}
