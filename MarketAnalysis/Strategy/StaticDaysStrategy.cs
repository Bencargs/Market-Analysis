using MarketAnalysis.Models;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : OptimisableStrategy
    {
        public int Identifier { get; set; }
        public override StrategyType StrategyType { get; } = StrategyType.StaticDates;
        protected override TimeSpan OptimisePeriod => TimeSpan.MaxValue;
        private readonly Dictionary<DateTime, bool> _buyDates;

        public StaticDatesStrategy(Dictionary<DateTime, bool> buyDates)
            : base(false)
        {
            _buyDates = buyDates;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return new IStrategy[0];
        }

        public override void SetParameters(IStrategy strategy)
        {
        }

        protected override bool ShouldBuy(MarketData data)
        {
            return _buyDates[data.Date];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StaticDatesStrategy strategy))
                return false;

            return strategy.Identifier == Identifier;
        }

        public override int GetHashCode()
        {
            return Identifier;
        }
    }
}
