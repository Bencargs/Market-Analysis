using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Strategy;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Services
{
    public class AnalysisService
    {
        private readonly IApiClient _apiClient;
        private readonly IProvider _dataProvider;
        private readonly IRepository _dataRepository;
        private readonly ICommunicationService _communicationService;

        public AnalysisService(
            IApiClient apiClient,
            IProvider dataProvider,
            IRepository dataRepository,
            ICommunicationService communicationService)
        {
            _apiClient = apiClient;
            _dataProvider = dataProvider;
            _dataRepository = dataRepository;
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

        private async Task<IEnumerable<Row>> GetPriceData()
        {
            var latestData = await _apiClient.GetData();
            var historicData = await _dataProvider.GetHistoricData();

            if (Configuration.InitialRun)
            {
                return historicData
                    .TakeWhile(x => x.Date < latestData.First().Date)
                    .Union(latestData);
            }
            var historicDates = new HashSet<DateTime>(historicData.Select(y => y.Date));
            return latestData.SkipWhile(x => historicDates.Contains(x.Date));
        }

        private ResultsProvider Simulate(IEnumerable<Row> data, IEnumerable<IStrategy> strategies)
        {
            Simulator simulator = Configuration.InitialRun 
                ? simulator = new Simulator(data, true)
                : simulator = new Simulator(new [] { data.Last() }, true); // wrong (should be last unprocessed data)

            var resultsProvider = new ResultsProvider();
            resultsProvider.Initialise(data);

            foreach (var s in strategies)
            {
                Log.Information($"Evaluating strategy: {s.GetType()}");
                using (var cache = SimulationCache.Instance)
                {
                    var history = simulator.Evaluate(s);
                    resultsProvider.AddResults(s, history);
                }
            }
            return resultsProvider;
        }
    }
}
