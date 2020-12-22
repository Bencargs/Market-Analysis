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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class YahooFinanceProvider : IApiDataProvider
    {
        private readonly string _url = Configuration.YahooApiEndpoint;
        private string _parameters => $"{Configuration.YahooQueryString}?period1={StartDatePeriod}&period2={GetEndDateString()}&interval=1d&events=history";
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<IEnumerable<MarketData>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            HttpClient.BaseAddress = new Uri(_url);
            var request = await HttpClient.GetAsync(_parameters);
            if (request.IsSuccessStatusCode)
            {
                var response = await request.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(response))
                using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                {
                    csv.Configuration.RegisterClassMap<YahooTimeSeriesDataMap>();
                    var records = csv.GetRecords<YahooTimeSeriesData>();
                    return ConvertToRow(records).ToArray();
                }
            }
            Log.Error("No response recieved from api data provider");
            return null;
        }

        private int StartDatePeriod => 1577836800;

        private string GetEndDateString()
        {
            var startDate = new DateTime(2020, 1, 1);
            var daysDiff = (DateTime.Today - startDate).Days;
            var startPeriod = StartDatePeriod;
            const int secondsPerDay = 86400;
            var endPeriod = startPeriod + (daysDiff * secondsPerDay);
            return endPeriod.ToString();
        }

        private IEnumerable<MarketData> ConvertToRow(IEnumerable<YahooTimeSeriesData> data)
        {
            MarketData lastData = null;
            foreach (var row in data)
            {
                var price = row.Close;
                if (price == 0 || price == null)
                    continue;
                
                var priceDelta = (lastData?.Price ?? 0m) - price;
                var volumeDelta = (lastData?.Volume ?? 0m) - row.Volume;

                var marketDataRow = new MarketData
                {
                    Date = row.Date,
                    Volume = (decimal)row.Volume,
                    Price = (decimal)price,
                    Delta = (decimal)priceDelta,
                    DeltaPercent = (decimal)(priceDelta != 0 && lastData?.Delta != null
                        ? (lastData.Delta - priceDelta) / priceDelta : 0),
                    VolumePercent = (decimal)(volumeDelta != 0 && lastData?.Volume != null
                        ? (lastData.Volume - volumeDelta) / volumeDelta : 0)
                };

                yield return marketDataRow;
                lastData = marketDataRow;
            }
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

        public class YahooTimeSeriesDataMap : ClassMap<YahooTimeSeriesData>
        {
            public YahooTimeSeriesDataMap()
            {
                Map(m => m.Date);
                Map(m => m.Close).TypeConverter<NullableValueConverter<decimal?>>();
                Map(m => m.Volume).TypeConverter<NullableValueConverter<int?>>();
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
                else if (type == typeof(int?))
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
