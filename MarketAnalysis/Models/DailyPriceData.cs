using Newtonsoft.Json;
using System.Collections.Generic;

namespace MarketAnalysis.Models
{
    public class DailyPriceData
    {
        [JsonProperty("Meta Data")]
        public PriceMetadata MetaData { get; set; }

        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, PriceTimeSeriesDaily> TimeSeriesDaily { get; set; }
    }
}
