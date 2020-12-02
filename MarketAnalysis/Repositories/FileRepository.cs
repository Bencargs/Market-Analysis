using CsvHelper;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Models.ApiData;
using MarketAnalysis.Strategy;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Repositories
{
    public class FileRepository : 
        IRepository<MarketData>,
        IRepository<IStrategy>,
        IRepository<SimulationResult>,
        IRepository<Investor>
    {
        private readonly string _dataFilePath = Configuration.DataPath;
        private readonly SimulationCache _simulationCache;
        private readonly MarketDataCache _marketDataCache;

        public FileRepository(
            MarketDataCache marketDataCache,
            SimulationCache simulationCache)
        {
            _simulationCache = simulationCache;
            _marketDataCache = marketDataCache;
        }

        Task<IEnumerable<MarketData>> IRepository<MarketData>.Get()
        {
            using var reader = new StreamReader(_dataFilePath);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration { HasHeaderRecord = false });
            var fileData = csv.GetRecords<FileMarketData>();
                
            var results = new List<MarketData>(5000);
            foreach (var row in fileData)
            {
                var lastData = results.LastOrDefault();
                var priceDelta = (lastData?.Price ?? 0m) - row.Price;
                var volumeDelta = (lastData?.Volume ?? 0m) - row.Volume;

                results.Add(new MarketData
                {
                    Date = row.Date,
                    Price = row.Price,
                    Delta = row.Delta,
                    Volume = row.Volume,
                    DeltaPercent = (priceDelta != 0 && lastData?.Delta != null)
                        ? (lastData.Delta - priceDelta) / priceDelta : 0,
                    VolumePercent = (volumeDelta != 0 && lastData?.Volume != null)
                        ? (lastData.Volume - volumeDelta) / volumeDelta : 0
                });
            }
            return Task.FromResult(results.OrderBy(x => x.Date).AsEnumerable());
        }

        public async Task Save(IEnumerable<MarketData> data)
        {
            using var writer = new StreamWriter(_dataFilePath, false);
            using var csv = new CsvWriter(writer);
            foreach (var d in data)
            {
                csv.WriteRecord(d);
                await csv.NextRecordAsync();
            }
        }

        async Task<IEnumerable<IStrategy>> IRepository<IStrategy>.Get()
        {
            var subStrategies = new IStrategy[]
            {
                //new EntropyStrategy(_marketDataCache),
                //new PatternRecognitionStrategy(_marketDataCache),
                new RelativeStrengthStrategy(_marketDataCache),
                new DeltaStrategy(),
                new GradientStrategy(_marketDataCache),
                new LinearRegressionStrategy(_marketDataCache),
                new VolumeStrategy(),
                //new MovingAverageStrategy(_marketDataCache),
                //new ClusteringStrategy(_marketDataCache)
            };
            var strategies = subStrategies.Concat(new[] { new WeightedStrategy(_simulationCache, subStrategies) });
            Log.Information($"Evaluating against strategies: {string.Join(", ", strategies)}");

            return await Task.FromResult(strategies);
        }

        public Task Save(IEnumerable<IStrategy> data)
        {
            throw new NotSupportedException();
        }

        Task<IEnumerable<SimulationResult>> IRepository<SimulationResult>.Get()
        {
            throw new NotSupportedException();
        }

        public async Task Save(IEnumerable<SimulationResult> results)
        {
            var resutlsFile = DirectoryManager.GetLatestResultsFile();
            var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(results, Formatting.Indented));
            File.WriteAllText(resutlsFile, json);
        }

        async Task<IEnumerable<Investor>> IRepository<Investor>.Get()
        {
            return await Task.FromResult(new[]
            {
                    new Investor
                    {
                        Name = "Benjamin Cargill",
                        Number = "000001",
                        Email = "benjamin.d.cargill@gmail.com",
                        DailyFunds = 10m,
                        OrderBrokerage = 0m,
                        OrderDelayDays = 3
                    },
                    //new RecipientDetails
                    //{
                    //    Date = DateTime.Now,
                    //    Name = "Cyndi Chen",
                    //    Number = "000002",
                    //    Email = "annsn12@hotmail.com"
                    //}
            });
        }

        public Task Save(IEnumerable<Investor> data)
        {
            throw new NotImplementedException();
        }
    }
}
