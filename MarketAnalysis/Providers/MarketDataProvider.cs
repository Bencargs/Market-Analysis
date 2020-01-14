using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class MarketDataProvider
    {
        private readonly ApiMarketDataProvider _apiDataProvider;
        private readonly IRepository<MarketData> _historicDataRespoitory;

        public MarketDataProvider(
            ApiMarketDataProvider apiDataProvider,
            IRepository<MarketData> historicDataRespoitory)
        {
            _apiDataProvider = apiDataProvider;
            _historicDataRespoitory = historicDataRespoitory;
        }

        public async Task<IEnumerable<MarketData>> GetPriceData()
        {
            var historicData = _historicDataRespoitory.Get();
            var latestData = _apiDataProvider.GetData();

            return JoinData(await historicData, await latestData);
        }

        private IEnumerable<MarketData> JoinData(IEnumerable<MarketData> historicData, IEnumerable<MarketData> latestData)
        {
            var firstOnlineDate = latestData.First().Date;
            historicData = historicData
                .TakeWhile(x => x.Date < firstOnlineDate);
            var joinPoint = latestData.First();
            joinPoint.Delta = joinPoint.Price - historicData.Last().Price;

            return historicData.Union(latestData);
        }
    }
}
