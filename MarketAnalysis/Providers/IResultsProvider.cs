using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;

namespace MarketAnalysis.Providers
{
    public interface IResultsProvider
    {
        void Initialise(IEnumerable<Row> data);
        void AddResults(IStrategy strategy, List<SimulationState> history);
        IEnumerable<SimulationResult> GetResults();
        bool ShouldBuy();
        decimal TotalProfit();
    }
}
