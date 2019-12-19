using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public class ConstantStrategy : OptimisableStrategy
    {
        protected override TimeSpan OptimisePeriod => TimeSpan.MaxValue;

        public ConstantStrategy() 
            : base(false)
        {
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return new IStrategy[0];
        }

        public override void SetParameters(IStrategy strategy)
        {
        }

        public override bool ShouldAddFunds()
        {
            return true;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            return true;
        }
    }
}
