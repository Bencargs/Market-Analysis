using MarketAnalysis.Models;
using MarketAnalysis.Models.ApiData;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class WorldTradingDataProvider : IApiDataProvider
    {
        private readonly string _url = Configuration.WorldApiEndpoint;
        private readonly string _parameters = $"/api/v1/{Configuration.WorldQueryString}&api_token={Configuration.WorldApiKey}";
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<IEnumerable<MarketData>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            HttpClient.BaseAddress = new Uri(_url);
            
            var response = await HttpClient.GetAsync(_parameters);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<WorldDailyPriceData>();
                if (result?.History != null)
                {
                    var rows = ConvertToRow(result.History);
                    return rows.OrderBy(x => x.Date);
                }
            }
            Log.Error("No response recieved from api data provider");
            return null;
        }

        private IEnumerable<MarketData> ConvertToRow(Dictionary<DateTime, WorldTimeSeriesData> response)
        {
            var results = new List<MarketData>(2000);
            foreach (var row in response)
            {
                var price = row.Value.Close;
                if (price == 0)
                    continue;

                var lastData = results.LastOrDefault();
                var priceDelta = (lastData?.Price ?? 0m) - price;
                var volumeDelta = (lastData?.Volume ?? 0m) - row.Value.Volume;

                results.Add(new MarketData
                {
                    Date = row.Key,
                    Volume = row.Value.Volume,
                    Price = price,
                    Delta = priceDelta,
                    DeltaPercent = priceDelta != 0 && lastData?.Delta != null 
                        ? (lastData.Delta - priceDelta) / priceDelta : 0,
                    VolumePercent = volumeDelta != 0 && lastData?.Volume != null 
                        ? (lastData.Volume - volumeDelta) / volumeDelta : 0
                });
            }
            return results;
        }
    }
}
