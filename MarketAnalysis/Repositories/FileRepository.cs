using CsvHelper;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
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
    public class FileRepository : IRepository, IProvider
    {
        private readonly string _dataFilePath = Configuration.DataPath;
        private readonly string _resultsFilePath = Configuration.ResultsPath;

        public async Task<IEnumerable<MarketData>> GetHistoricData()
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

        public async Task<IEnumerable<IStrategy>> GetStrategies()
        {
            if (!File.Exists(_resultsFilePath))
                File.Create(_resultsFilePath);

            IEnumerable<IStrategy> strategies = null;
            if (!Configuration.InitialRun)
            {
                using (var reader = new StreamReader(_resultsFilePath))
                {
                    var json = await reader.ReadToEndAsync();
                    strategies = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<SimulationResult>>(json)?.Select(x =>
                    {
                        var type = Type.GetType(x.StrategyType);
                        return (IStrategy)JsonConvert.DeserializeObject(x.Strategy, type);
                    }));
                }
            }
            if (strategies == null)
            {
                var subStrategies = new IStrategy[]
                {
                    new PatternRecognitionStrategy(800),
                    new RelativeStrengthStrategy(50),
                    new DeltaStrategy(1),
                    new GradientStrategy(20, -0.06m),
                    new LinearRegressionStrategy(149),
                    new VolumeStrategy(182),
                };
                strategies = subStrategies.Concat(new[] { new MultiStrategy(subStrategies) });
            }
            Log.Information($"Evaluating against strategies: {string.Join(", ", strategies)}");
            return strategies;
        }

        public async Task<string> GetEmailTemplate()
        {
            var path = Configuration.EmailTemplatePath;
            return await File.ReadAllTextAsync(path);
        }

        public async Task<IEnumerable<RecipientDetails>> GetEmailRecipients()
        {
            return new[]
            {
                new RecipientDetails
                {
                    Date = DateTime.Now,
                    Name = "Client Name",
                    Number = "000001",
                    Email = "client@email.com"
                }
            };
        }

        public async Task SaveData(IEnumerable<MarketData> data)
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

        public async Task SaveSimulationResults(IResultsProvider resultsProvider)
        {
            var results = resultsProvider.GetResults();
            var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(results));
            File.WriteAllText(_resultsFilePath, json);
        }
    }
}
