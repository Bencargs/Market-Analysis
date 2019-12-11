using MarketAnalysis.Models;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        object Key { get; }
        bool ShouldOptimise();
        IEnumerable<IStrategy> GetOptimisations();
        void SetParameters(IStrategy strategy);
        bool ShouldAddFunds();
        bool ShouldBuyShares(MarketData data);
    }
}
