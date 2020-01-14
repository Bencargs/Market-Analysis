using MarketAnalysis.Repositories;
using MarketAnalysis.Strategy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class StrategyProvider
    {
        private readonly IRepository<IStrategy> _strategyRepository;

        public StrategyProvider(IRepository<IStrategy> strategyRepository)
        {
            _strategyRepository = strategyRepository;
        }

        public Task<IEnumerable<IStrategy>> GetStrategies()
        {
            return _strategyRepository.Get();
        }
    }
}
