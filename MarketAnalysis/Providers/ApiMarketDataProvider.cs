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
        private string _parameters = $"/query?{Configuration.QueryString}&apikey={Configuration.ApiKey}";

        public async Task<IEnumerable<Row>> GetData()
        {
            Log.Information($"Reading market data from provider {_url}");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(_parameters);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<DailyPriceData>();
                    if (result.TimeSeriesDaily != null)
                    {
                        var rows = ConvertToRow(result);
                        return rows.OrderBy(x => x.Date);
                    }
                }
            }
            Log.Error("No response recieved from api data provider");
            return null;
        }

        private IEnumerable<Row> ConvertToRow(DailyPriceData response)
        {
            var results = new List<Row>();
            foreach (var row in response?.TimeSeriesDaily)
            {
                var price = row.Value.Open;
                if (price == 0)
                    continue;

                var adjustment = Configuration.OnlinePriceAdjustment;
                // All historic data sets were trained with the All Ords dataset
                // Unfortunately the online data provider does not track this index
                // the all market VAS index can be used, with this adjustment to approximate

                results.Add(new Row
                {
                    Date = DateTime.Parse(row.Key),
                    Volume = row.Value.Volume,
                    Price = price / adjustment,
                    Delta = price - (results.LastOrDefault()?.Price ?? 0m)
                });
            }
            return results;
        }
    }
}
