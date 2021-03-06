﻿using CsvHelper;
using CsvHelper.Configuration;
using MarketAnalysis.Models;
using MarketAnalysis.Models.ApiData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Repositories
{
    public class FileRepository : 
        IRepository<MarketData>,
        IRepository<SimulationResult>
    {
        private readonly string _dataFilePath = Configuration.DataPath;

        async Task<IEnumerable<MarketData>> IRepository<MarketData>.Get()
        {
            var results = new List<MarketData>(5000);

            using var reader = new StreamReader(_dataFilePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = false });
            var fileData = csv.GetRecordsAsync<FileMarketData>();
            await foreach (var row in fileData)
            {
                var lastData = results.LastOrDefault();
                var priceDelta = (lastData?.Price ?? 0m) - row.Price;
                var volumeDelta = (lastData?.Volume ?? 0m) - row.Volume;
                var spreadDelta = (lastData?.Spread ?? 0m) - row.Spread;

                results.Add(new MarketData
                {
                    Date = row.Date,
                    Price = row.Price,
                    Delta = row.Delta,
                    Volume = row.Volume,
                    Spread = row.Spread,
                    DeltaPercent = priceDelta != 0 && lastData?.Delta != null
                        ? (lastData.Delta - priceDelta) / priceDelta : 0,
                    VolumePercent = volumeDelta != 0 && lastData?.Volume != null
                        ? (lastData.Volume - volumeDelta) / volumeDelta : 0,
                    SpreadPercent = spreadDelta != 0 && lastData?.Spread != null
                        ? (lastData.Spread - spreadDelta) / spreadDelta : 0,
                });
            }
            return results.OrderBy(x => x.Date);
        }

        public async Task Save(IEnumerable<MarketData> data)
        {
            await using var writer = new StreamWriter(_dataFilePath, false);
            await using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
            foreach (var d in data)
            {
                csv.WriteRecord(d);
                await csv.NextRecordAsync();
            }
        }

        Task<IEnumerable<SimulationResult>> IRepository<SimulationResult>.Get()
        {
            throw new NotSupportedException();
        }

        public async Task Save(IEnumerable<SimulationResult> results)
        {
            var resultsFile = DirectoryManager.GetLatestResultsFile();
            var json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(results, Formatting.Indented));
            await File.WriteAllTextAsync(resultsFile, json);
        }
    }
}
