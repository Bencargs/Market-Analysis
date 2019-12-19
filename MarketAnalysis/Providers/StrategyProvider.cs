using MarketAnalysis.Repositories;
using MarketAnalysis.Strategy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class StrategyProvider
    {
        private IRepository<IStrategy> _strategyRepository;

        public StrategyProvider(IRepository<IStrategy> strategyRepository)
        {
            _strategyRepository = strategyRepository;
        }

        public async Task<IEnumerable<IStrategy>> GetStrategies()
        {
            return await _strategyRepository.Get();
        }
    }
}
