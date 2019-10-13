using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public interface ICommunicationService<T>
    {
        Task SendCommunication(IEnumerable<T> results);
    }
}
