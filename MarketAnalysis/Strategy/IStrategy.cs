using MarketAnalysis.Models;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        object Key { get; }
        bool ShouldOptimise();
        IEnumerable<IStrategy> Optimise();
        bool ShouldAddFunds();
        bool ShouldBuyShares(MarketData data);
    }
}
