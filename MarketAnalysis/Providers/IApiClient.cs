using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IApiClient
    {
        Task<IEnumerable<MarketData>> GetData();
    }
}
