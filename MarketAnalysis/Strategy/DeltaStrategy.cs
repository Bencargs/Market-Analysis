﻿using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;
using MarketAnalysis.Staking;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy, IEquatable<DeltaStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IStakingService _stakingService;
        private DeltaParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Delta;

        public DeltaStrategy(
            ISearcher searcher,
            IStakingService stakingService,
            DeltaParameters parameters)
        {
            _searcher = searcher;
            _stakingService = stakingService;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            _stakingService.Evaluate(fromDate, endDate);
            
            var potentials = Enumerable.Range(1, 100).Select(x =>
            {
                var threshold = (decimal)x / 1000;
                return new DeltaParameters { Threshold = threshold };
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (DeltaParameters) optimum;
        }

        public bool ShouldBuy(MarketData data)
        {
            return data.Delta < _parameters.Threshold;
        }

        public decimal GetStake(DateTime today, decimal totalFunds)
        {
            return _stakingService.GetStake(today, totalFunds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DeltaStrategy)) return false;

            return Equals(obj as DeltaStrategy);
        }

        public bool Equals(DeltaStrategy strategy)
        {
            return strategy._parameters.Threshold == _parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold);
        }
    }
}
