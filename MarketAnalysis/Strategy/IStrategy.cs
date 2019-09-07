using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        void Optimise();
        bool ShouldAddFunds();
        bool ShouldBuyShares(Row data);
    }
}
