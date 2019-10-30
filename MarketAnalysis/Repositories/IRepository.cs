using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Repositories
{
    public interface IRepository
    {
        Task SaveData(IEnumerable<MarketData> data);
        Task SaveSimulationResults(IResultsProvider results);
    }
}
