using Newtonsoft.Json;
using System.Collections.Generic;

namespace MarketAnalysis.Models.ApiData
{
    public class AlphaDailyPriceData
    {
        [JsonProperty("Meta Data")]
        public PriceMetadata MetaData { get; set; }

        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, PriceTimeSeriesDaily> TimeSeriesDaily { get; set; }
    }
}
