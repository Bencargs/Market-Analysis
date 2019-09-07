using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Repositories
{
    public interface IRepository
    {
        Task SaveData(IEnumerable<Row> data);
        Task SaveSimulationResults(IEnumerable<SimulationResult> results);
    }
}
