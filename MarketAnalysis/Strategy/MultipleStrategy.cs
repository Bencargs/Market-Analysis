using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Staking;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class MultipleParameters : IParameters
    {
        public int Threshold { get; set; }
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(512);
        public List<IStrategy> Strategies { get; set; } = new();
    }

    public class MultipleStrategy : IStrategy, IAggregateStrategy, IEquatable<MultipleStrategy>
    {
        private readonly IStakingService _stakingService;
        private MultipleParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Multiple;

        public MultipleStrategy(
            IStakingService stakingService,
            MultipleParameters parameters)
        {
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public decimal GetStake(DateTime today, decimal totalFunds)
        {
            return _stakingService.GetStake(today, totalFunds);
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            //var potentials = Enumerable.Range(1, _parameters.Strategies.Count)
            //    .Select(x => new MultipleParameters
            //    {
            //        Strategies = _parameters.Strategies,
            //        Threshold = x
            //    });

            //var optimum = _searcher.Maximum(potentials, fromDate, toDate);

            //_parameters = (MultipleParameters)optimum;
        }

        public bool ShouldBuy(MarketData data)
        {
            var count = _parameters.Strategies.Where(x => x.ShouldBuy(data)).Count();
            
            return count >= _parameters.Threshold;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MultipleStrategy)) return false;

            return Equals(obj as MultipleStrategy);
        }

        public bool Equals(MultipleStrategy strategy)
        {
            if (_parameters.Threshold != strategy._parameters.Threshold)
                return false;

            return _parameters.Strategies.Count == strategy._parameters.Strategies.Count &&
                   _parameters.Strategies.All(strategy._parameters.Strategies.Contains);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold, _parameters.Strategies);
        }
    }
}
