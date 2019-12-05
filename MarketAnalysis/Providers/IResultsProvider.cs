using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;

namespace MarketAnalysis.Providers
{
    public interface IResultsProvider
    {
        void Initialise();
        void AddResults(IStrategy strategy, IEnumerable<SimulationState> history);
        IEnumerable<SimulationResult> GetResults();
        bool ShouldBuy();
        decimal TotalProfit();
    }
}
