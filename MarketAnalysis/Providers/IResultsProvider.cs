using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IResultsProvider
    {
        void Initialise();
        void AddResults(Investor investor, Dictionary<IStrategy , SimulationState[]> history);
        Dictionary<Investor, IEnumerable<SimulationResult>> GetResults();
        Task SaveSimulationResults();
        Task SaveData(IEnumerable<MarketData> data);
    }
}
