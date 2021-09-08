using System;
using System.Linq;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Staking;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class SpreadStrategy : IStrategy, IEquatable<SpreadStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IStakingService _stakingService;
        private SpreadParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Spread;

        public SpreadStrategy(
            ISearcher searcher,
            IStakingService stakingService,
            SpreadParameters parameters)
        {
            _searcher = searcher;
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            _stakingService.Evaluate(fromDate, toDate);

            var potentials = Enumerable.Range(0, 100)
                .Select(x => new SpreadParameters {Threshold = (decimal)x/100});

            var optimum = _searcher.Maximum(potentials, fromDate, toDate);

            _parameters = (SpreadParameters) optimum;
        }

        public bool ShouldBuy(MarketData data)
            => data.SpreadPercent > _parameters.Threshold;

        public decimal GetStake(decimal totalFunds)
        {
            return _stakingService.GetStake(totalFunds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(SpreadStrategy)) return false;

            return Equals(obj as SpreadStrategy);
        }

        public bool Equals(SpreadStrategy other)
            => other._parameters.Threshold == _parameters.Threshold;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Threshold);
    }
}
