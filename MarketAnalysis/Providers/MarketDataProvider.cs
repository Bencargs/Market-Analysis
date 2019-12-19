using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class MarketDataProvider
    {
        private ApiMarketDataProvider _apiDataProvider;
        private IRepository<MarketData> _historicDataRespoitory;

        public MarketDataProvider(
            ApiMarketDataProvider apiDataProvider,
            IRepository<MarketData> historicDataRespoitory)
        {
            _apiDataProvider = apiDataProvider;
            _historicDataRespoitory = historicDataRespoitory;
        }

        public async Task<IEnumerable<MarketData>> GetPriceData()
        {
            var historicData = await _historicDataRespoitory.Get();
            var latestData = await _apiDataProvider.GetData();

            var firstOnlineDate = latestData.First().Date;
            return historicData
                .TakeWhile(x => x.Date < firstOnlineDate)
                .Union(latestData);
        }
    }
}
