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
        private readonly string _readonlyDataFilePath = Configuration.ReadonlyDataPath;
        private readonly string _resultsFilePath = Configuration.ResultsPath;
        private readonly string _emailTemplateFilePath = Configuration.EmailTemplatePath;

        public async Task<IEnumerable<Row>> GetHistoricData()
        {
            Log.Information($"Loading historic market data from {_readonlyDataFilePath}");
            var results = new List<Row>();
            using (var reader = new StreamReader(_readonlyDataFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line.Split(',');

                    var provider = System.Globalization.CultureInfo.InvariantCulture;
                    var dateStr = values[0].ToString().Replace("\"", "");
                    var date = DateTime.ParseExact(dateStr, "MMM-dd-yyyy", provider);

                    var priceStr = values[1].ToString().Replace("\"", "");
                    if (!decimal.TryParse(priceStr, out decimal price))
                        continue;

                    var volumeStr = values[5].ToString().Replace("\"", "");
                    decimal.TryParse(volumeStr.Replace("M", "").Replace("B", ""), out decimal volume);
                    if (volumeStr.Last() == 'B')
                        volume = volume * 1000;

                    results.Add(new Row
                    {
                        Date = date,
                        Price = price,
                        Volume = volume
                    });
                }
            }
            results = results.OrderBy(x => x.Date).ToList();
            for (int i = 1; i < results.Count; i++)
            {
                var delta = results[i].Price - results[i - 1].Price;
                results[i].Delta = delta;
            }
            return results;
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
            return await File.ReadAllTextAsync(_emailTemplateFilePath);
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

        public async Task SaveData(IEnumerable<Row> data)
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

        public async Task SaveSimulationResults(IEnumerable<SimulationResult> results)
        {
            var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(results));
            File.WriteAllText(_resultsFilePath, json);
        }
    }
}
