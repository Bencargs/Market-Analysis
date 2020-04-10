using MarketAnalysis.Models;
using MarketAnalysis.Simulation;
using ShellProgressBar;
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

        protected override IStrategy GetOptimum(ISimulator _, IProgressBar __)
        {
            return null;
        }

        protected override void SetParameters(IStrategy strategy)
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
