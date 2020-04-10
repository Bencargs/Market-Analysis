using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        StrategyType StrategyType { get; }
        bool ShouldBuyShares(MarketData data);
    }
}
