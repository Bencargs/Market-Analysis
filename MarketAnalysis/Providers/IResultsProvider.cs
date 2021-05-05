using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IResultsProvider
    {
        void Initialise();
        void AddResults(Investor investor, ConcurrentDictionary<IStrategy , SimulationState[]> history);
        Dictionary<Investor, IEnumerable<SimulationResult>> GetResults();
        decimal MarketAverage();
        Task SaveSimulationResults();
        Task SaveChart(ResultsChart chartType, string path);
        Task SaveData(IEnumerable<MarketData> data);
    }
}
