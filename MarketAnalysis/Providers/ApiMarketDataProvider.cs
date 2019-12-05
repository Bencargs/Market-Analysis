using MarketAnalysis.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class ApiMarketDataProvider : IApiClient
    {
        private string _url = Configuration.ApiEndpoint;
        private string  _parameters = $"/query?{Configuration.QueryString}&apikey={Configuration.ApiKey}";
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<IEnumerable<MarketData>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            HttpClient.BaseAddress = new Uri(_url);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await HttpClient.GetAsync(_parameters);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<DailyPriceData>();
                if (result.TimeSeriesDaily != null)
                {
                    var rows = ConvertToRow(result);
                    return rows.OrderBy(x => x.Date);
                }
            }
            Log.Error("No response recieved from api data provider");
            return null;
        }

        private IEnumerable<MarketData> ConvertToRow(DailyPriceData response)
        {
            var results = new List<MarketData>(2000);
            foreach (var row in response?.TimeSeriesDaily)
            {
                var price = row.Value.Open;
                if (price == 0)
                    continue;

                results.Add(new MarketData
                {
                    Date = DateTime.Parse(row.Key),
                    Volume = row.Value.Volume,
                    Price = price,
                    Delta = price - (results.LastOrDefault()?.Price ?? 0m)
                });
            }
            return results;
        }
    }
}
