using CsvHelper;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
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
        IRepository<EmailTemplate>
    {
        private readonly string _dataFilePath = Configuration.DataPath;
        private readonly string _resultsFilePath = Configuration.ResultsPath;
        private readonly SimulationCache _cache;

        public FileRepository(SimulationCache cache)
        {
            _cache = cache;
        }

        async Task<IEnumerable<MarketData>> IRepository<MarketData>.Get()
        {
            return await Task.Run(() =>
            {
                using (var reader = new StreamReader(_dataFilePath))
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration { HasHeaderRecord = false }))
                {
                    return csv.GetRecords<MarketData>().ToArray();
                }
            });
        }

        public async Task Save(IEnumerable<MarketData> data)
        {
            using (var writer = new StreamWriter(_dataFilePath, false))
            using (var csv = new CsvWriter(writer))
            {
                foreach (var d in data)
                {
                    csv.WriteRecord(d);
                    await csv.NextRecordAsync();
                }
            }
        }

        async Task<IEnumerable<IStrategy>> IRepository<IStrategy>.Get()
        {
            // todo: this is in the wrong place?
            if (!File.Exists(_resultsFilePath))
                File.Create(_resultsFilePath).Dispose();

            var subStrategies = new IStrategy[]
            {
                new EntropyStrategy(30, 5),
                new PatternRecognitionStrategy(800),
                new RelativeStrengthStrategy(50),
                new DeltaStrategy(0.05m),
                new GradientStrategy(20, -0.06m),
                new LinearRegressionStrategy(149),
                new VolumeStrategy(182),
            };
            var strategies = subStrategies.Concat(new[] { new WeightedStrategy(_cache, subStrategies, 0.5d) });
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
            var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(results));
            File.WriteAllText(_resultsFilePath, json);
        }

        async Task<IEnumerable<EmailTemplate>> IRepository<EmailTemplate>.Get()
        {
            var path = Configuration.EmailTemplatePath;
            var body = await File.ReadAllTextAsync(path);
            return new[]
            {
                new EmailTemplate
                {
                    Body = body
                }
            };
        }

        public Task Save(IEnumerable<EmailTemplate> data)
        {
            throw new NotImplementedException();
        }
    }
}
