using MarketAnalysis.Models;
using System.Collections.Generic;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        StrategyType StrategyType { get; }
        bool ShouldOptimise();
        IEnumerable<IStrategy> GetOptimisations();
        void SetParameters(IStrategy strategy);
        bool ShouldBuyShares(MarketData data);
    }
}
