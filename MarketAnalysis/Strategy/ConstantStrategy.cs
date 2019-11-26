using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public class ConstantStrategy : IStrategy
    {
        public object Key => new object();

        public bool ShouldOptimise()
        {
            return false;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return new IStrategy[0];
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            return true;
        }
    }
}
