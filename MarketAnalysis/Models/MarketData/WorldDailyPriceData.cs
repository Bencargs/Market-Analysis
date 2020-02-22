using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Models.ApiData
{
    public class WorldDailyPriceData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("history")]
        public Dictionary<DateTime, WorldTimeSeriesData> History { get; set; }
    }
}
