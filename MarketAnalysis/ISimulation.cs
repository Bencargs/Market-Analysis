using MarketAnalysis.Models;
using MarketAnalysis.Strategy;

namespace MarketAnalysis
{
    public interface ISimulation
    {
        int BuyCount { get; }
        SimulationResult Evaluate(IStrategy strategy);
    }
}
