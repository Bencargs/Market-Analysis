using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IProvider
    {
        Task<IEnumerable<Row>> GetHistoricData();
        Task<IEnumerable<IStrategy>> GetStrategies();
        Task<EmailTemplate> GetEmailTemplate();
    }
}
