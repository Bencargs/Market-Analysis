using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public class ConstantStrategy : IStrategy
    {
        public bool ShouldOptimise()
        {
            return false;
        }

        public IEnumerable<IStrategy> GetOptimisations()
        {
            return new IStrategy[0];
        }

        public void SetParameters(IStrategy strategy)
        {
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
