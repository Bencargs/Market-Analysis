using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        object Key { get; }
        bool ShouldOptimise();
        void Optimise();
        bool ShouldAddFunds();
        bool ShouldBuyShares(Row data);
    }
}
