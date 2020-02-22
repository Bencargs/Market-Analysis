using MarketAnalysis.Models;
using MarketAnalysis.Models.ApiData;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class AlphaVantageDataProvider : IApiDataProvider
    {
        private readonly string _url = Configuration.AlphaApiEndpoint;
        private readonly string _parameters = $"/query?{Configuration.AlphaQueryString}&apikey={Configuration.AlphaApiKey}";
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<IEnumerable<MarketData>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            HttpClient.BaseAddress = new Uri(_url);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await HttpClient.GetAsync(_parameters);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<AlphaDailyPriceData>();
                if (result?.TimeSeriesDaily != null)
                {
                    var rows = ConvertToRow(result);
                    return rows.OrderBy(x => x.Date);
                }
            }
            Log.Error("No response recieved from api data provider");
            return null;
        }

        private IEnumerable<MarketData> ConvertToRow(AlphaDailyPriceData response)
        {
            var results = new List<MarketData>(2000);
            foreach (var row in response?.TimeSeriesDaily)
            {
                var price = row.Value.Open;
                if (price == 0)
                    continue;

                var lastData = results.LastOrDefault();
                var priceDelta = price - (lastData?.Price ?? 0m);
                var volumeDelta = row.Value.Volume - (lastData?.Volume ?? 0m);

                results.Add(new MarketData
                {
                    Date = DateTime.Parse(row.Key),
                    Volume = row.Value.Volume,
                    Price = price,
                    Delta = priceDelta,
                    DeltaPercent = lastData?.Delta != 0 
                        ? (priceDelta - lastData.Delta) / lastData.Delta : 0,
                    VolumePercent = lastData?.Volume != 0 
                        ? (volumeDelta - lastData.Volume) / lastData.Volume : 0
                });
            }
            return results;
        }
    }
}
