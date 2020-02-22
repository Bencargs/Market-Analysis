using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IApiDataProvider
    {
        Task<IEnumerable<MarketData>> GetData();
    }
}
