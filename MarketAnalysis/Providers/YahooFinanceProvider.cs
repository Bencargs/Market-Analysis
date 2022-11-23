using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using MarketAnalysis.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class YahooFinanceProvider : IApiDataProvider
    {
        private static int StartDatePeriod => 1577836800;
        private readonly string _url = Configuration.YahooApiEndpoint;
        private static string Parameters => $"{Configuration.YahooQueryString}?period1={StartDatePeriod}&period2={GetEndDateString()}&interval=1d&events=history";

        public async Task<IEnumerable<MarketData>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            var client = new HttpClient { BaseAddress = new Uri(_url) };
            var request = await client.GetAsync(Parameters);
            if (request.IsSuccessStatusCode)
            {
                await using var response = await request.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(response);
                using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
                csv.Context.RegisterClassMap<YahooTimeSeriesDataMap>();
                var records = csv.GetRecordsAsync<YahooTimeSeriesData>();
                var results = await ConvertToRow(records);
                return results;
            }
            Log.Error("No response received from api data provider");
            return null;
        }

        private static async Task<List<MarketData>> ConvertToRow(IAsyncEnumerable<YahooTimeSeriesData> records)
        {
            var results = new List<MarketData>();
            MarketData lastData = null;
            await foreach (var row in records)
            {
                var price = row.Close ?? 0;
                if (price == 0)
                    continue;
                var volume = row.Volume ?? 0;
                var spread = row.High - row.Low ?? 0;

                var priceDelta = (lastData?.Price ?? 0m) - price;
                var volumeDelta = (lastData?.Volume ?? 0m) - volume;
                var spreadDelta = (lastData?.Spread ?? 0m) - spread;

                var marketDataRow = new MarketData
                {
                    Date = row.Date,
                    Volume = volume,
                    Price = price,
                    Delta = priceDelta,
                    Spread = spread,
                    DeltaPercent = priceDelta != 0 && lastData?.Delta != null
                        ? (lastData.Delta - priceDelta) / priceDelta : 0,
                    VolumePercent = volumeDelta != 0 && lastData?.Volume != null
                        ? (lastData.Volume - volumeDelta) / volumeDelta : 0,
                    SpreadPercent = spreadDelta != 0 && lastData?.Spread != null
                        ? (lastData.Spread - spreadDelta) / spreadDelta : 0
                };

                results.Add(marketDataRow);
                lastData = marketDataRow;
            }

            return results;
        }

        private static string GetEndDateString()
        {
            var startDate = new DateTime(2020, 1, 1);
            var daysDiff = (DateTime.Today - startDate).Days;
            var startPeriod = StartDatePeriod;
            const int secondsPerDay = 86400;
            var endPeriod = startPeriod + (daysDiff * secondsPerDay);
            return endPeriod.ToString();
        }

        public class YahooTimeSeriesData
        {
            public DateTime Date { get; set; }
            public decimal? Open { get; set; }
            public decimal? High { get; set; }
            public decimal? Low { get; set; }
            public decimal? Close { get; set; }
            [Name("Adj Close")]
            public decimal? AdjClose { get; set; }
            public int? Volume { get; set; }
        }

        public sealed class YahooTimeSeriesDataMap : ClassMap<YahooTimeSeriesData>
        {
            public YahooTimeSeriesDataMap()
            {
                Map(m => m.Date);
                Map(m => m.Close).TypeConverter<NullableValueConverter<decimal?>>();
                Map(m => m.Volume).TypeConverter<NullableValueConverter<int?>>();
                Map(m => m.High).TypeConverter<NullableValueConverter<decimal?>>();
                Map(m => m.Low).TypeConverter<NullableValueConverter<decimal?>>();
            }
        }

        public class NullableValueConverter<T> : DefaultTypeConverter
        {
            public override object ConvertFromString(string test, IReaderRow row, MemberMapData memberMapData)
            {
                var type = typeof(T);
                var defaultType = default(T);
                if (string.IsNullOrWhiteSpace(test))
                    return null;

                if (type == typeof(decimal?))
                {
                    return (decimal.TryParse(test, out var dResult))
                        ? (decimal?)dResult
                        : null;
                }

                if (type == typeof(int?))
                {
                    return (int.TryParse(test, out var iResult))
                        ? (int?)iResult
                        : null;
                }
                return defaultType;
            }
        }
    }
}
