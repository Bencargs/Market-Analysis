using Newtonsoft.Json;

namespace MarketAnalysis.Models.ApiData
{
    public class WorldTimeSeriesData
    {
        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }
    }
}
