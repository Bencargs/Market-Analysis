using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public interface ICommunicationService
    {
        Task SendCommunication(IResultsProvider results);
    }
}
