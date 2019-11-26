using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly IApiClient _apiClient;
        private readonly ISimulator _simulator;
        private readonly IProvider _dataProvider;
        private readonly IRepository _dataRepository;
        private readonly IResultsProvider _resultsProvider;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            IApiClient apiClient,
            ISimulator simulator,
            IProvider dataProvider,
            IRepository dataRepository,
            IResultsProvider resultsProvider,
            ICommunicationService communicationService)
        {
            _apiClient = apiClient;
            _simulator = simulator;
            _dataProvider = dataProvider;
            _dataRepository = dataRepository;
            _resultsProvider = resultsProvider;
            _communicationService = communicationService;
        }

        public async Task Execute()
        {
            var data = await GetPriceData();
            var strategies = await _dataProvider.GetStrategies();

            var results = Simulate(data, strategies);

            if (results.ShouldBuy())
                await _communicationService.SendCommunication(results);
                
            await _dataRepository.SaveSimulationResults(results);
            await _dataRepository.SaveData(data);
        }

        private async Task<IEnumerable<MarketData>> GetPriceData()
        {
            var historicData = await _dataProvider.GetHistoricData();
            var latestData = await _apiClient.GetData();

            var firstOnlineDate = latestData.First().Date;
            return historicData
                .TakeWhile(x => x.Date < firstOnlineDate)
                .Union(latestData);
        }

        private IResultsProvider Simulate(IEnumerable<MarketData> data, IEnumerable<IStrategy> strategies)
        {
            using (var cache = MarketDataCache.Instance)
            {
                cache.Initialise(data);
                _resultsProvider.Initialise();

                foreach (var s in strategies)
                {
                    Log.Information($"Evaluating strategy: {s.GetType()}");
                    var history = _simulator.Evaluate(s);
                    _resultsProvider.AddResults(s, history);
                }
            }
            return _resultsProvider;
        }
    }
}
