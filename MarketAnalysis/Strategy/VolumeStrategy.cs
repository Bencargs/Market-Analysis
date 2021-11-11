using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;
using MarketAnalysis.Staking;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : IStrategy, IEquatable<VolumeStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IStakingService _stakingService;
        private VolumeParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Volume;

        public VolumeStrategy(
            ISearcher searcher,
            IStakingService stakingService,
            VolumeParameters parameters)
        {
            _searcher = searcher;
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            _stakingService.Evaluate(fromDate, endDate);
            
            var potentials = Enumerable.Range(1, 800).Select(x =>
                new VolumeParameters { Threshold = x });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (VolumeParameters) optimum;
        }

        public bool ShouldBuy(MarketData data)
        {
            var shouldBuy = _parameters.PreviousVolume != 0 &&
                (data.Volume / _parameters.PreviousVolume) > _parameters.Threshold;

            _parameters.PreviousVolume = data.Volume;
            return shouldBuy;
        }

        public decimal GetStake(DateTime today, decimal totalFunds)
        {
            return _stakingService.GetStake(today, totalFunds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolumeStrategy)) return false;

            return Equals(obj as VolumeStrategy);
        }

        public bool Equals(VolumeStrategy strategy)
        {
            return _parameters.Threshold == strategy._parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold);
        }
    }
}
