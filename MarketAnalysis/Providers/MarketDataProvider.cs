using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class MarketDataProvider
    {
        private readonly IApiDataProvider _apiDataProvider;
        private readonly IRepository<MarketData> _historicDataRepository;

        public MarketDataProvider(
            IApiDataProvider apiDataProvider,
            IRepository<MarketData> historicDataRepository)
        {
            _apiDataProvider = apiDataProvider;
            _historicDataRepository = historicDataRepository;
        }

        public async Task<IEnumerable<MarketData>> GetPriceData()
        {
            var historicData = _historicDataRepository.Get();
            var latestData = _apiDataProvider.GetData();
            await Task.WhenAll(historicData, latestData);

            return JoinData(historicData.Result, latestData.Result);
        }

        private static IEnumerable<MarketData> JoinData(IEnumerable<MarketData> historicData, IEnumerable<MarketData> latestData)
        {
            var lastHistoricData = historicData.Last();
            var recentData = latestData.Where(x => x.Date > lastHistoricData.Date).ToArray();
            if (!recentData.Any())
                return historicData;
            
            var joinPoint = recentData.First();
            joinPoint.Delta = joinPoint.Price - lastHistoricData.Price;
            joinPoint.DeltaPercent = joinPoint.Delta != 0 ?(lastHistoricData.Delta - joinPoint.Delta) / joinPoint.Delta : 0;
            joinPoint.VolumePercent = (lastHistoricData.Volume - joinPoint.Volume) / joinPoint.Volume;

            return historicData.Union(recentData).ToArray();
        }
    }
}
