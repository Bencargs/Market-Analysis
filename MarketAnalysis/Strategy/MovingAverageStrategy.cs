using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using System;
using System.Linq;
using MarketAnalysis.Staking;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class MovingAverageStrategy : IStrategy, IEquatable<MovingAverageStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IMarketDataCache _marketDataCache;
        private readonly IStakingService _stakingService;
        private MovingAverageParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.MovingAverage;

        public MovingAverageStrategy(
            IMarketDataCache marketDataCache,
            IStakingService stakingService,
            ISearcher searcher,
            MovingAverageParameters parameters)
        {
            _searcher = searcher;
            _marketDataCache = marketDataCache;
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            _stakingService.Evaluate(fromDate, endDate);

            var potentials = Enumerable.Range(1, 90).SelectMany(w =>
            {
                return Enumerable.Range(1, 60).Select(t =>
                {
                    var threshold = (double)t / 10;
                    return new MovingAverageParameters { Window = w, Threshold = threshold };
                });
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (MovingAverageParameters)optimum;
        }

        public bool ShouldBuy(MarketData data)
        {
            var batch = _marketDataCache.GetLastSince(data.Date, _parameters.Window)
                .Select(x => x.Price)
                .ToArray();
            
            if (batch.Length < 2)
                return false;

            var mean = batch.Average();
            var sum = batch.Sum(d => Math.Pow((double)(d - mean), 2));
            var a = Math.Abs(sum / batch.Length - 1);
            var standardDeviation = Math.Sqrt(a);
            var weightedDeviation = (decimal)(standardDeviation * _parameters.Threshold);

            return data.Price < mean - weightedDeviation;
        }

        public decimal GetStake(decimal totalFunds)
        {
            return _stakingService.GetStake(totalFunds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MovingAverageStrategy)) return false;

            return Equals(obj as MovingAverageStrategy);
        }

        public bool Equals(MovingAverageStrategy strategy)
            => strategy._parameters.Window == _parameters.Window && 
               Math.Abs(strategy._parameters.Threshold - _parameters.Threshold) < 0.00001;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Window, _parameters.Threshold);
    }
}
