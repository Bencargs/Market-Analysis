using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Repositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> Get();
        Task Save(IEnumerable<T> data);
    }
}
