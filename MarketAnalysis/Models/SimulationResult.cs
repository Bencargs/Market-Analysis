using MarketAnalysis.Strategy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Models
{
    public class SimulationResult
    {
        public DateTime Date { get; set; }
        public decimal ProfitYTD { get; set; }
        public decimal ProfitTotal { get; set; }
        public decimal AboveMarketReturn { get; set; }
        public decimal Alpha { get; set; }
        public decimal MaximumAlpha { get; set; }
        public decimal MaximumDrawdown { get; set; }
        public decimal Frequency { get; set; }
        public decimal Accuracy { get; set; }
        public decimal Recall { get; set; }
        public decimal Precision { get; set; }
        public Dictionary<ConfusionCategory, int> ConfusionMatrix { get; set; }
        public bool ShouldBuy { get; set; }
        public string Strategy { get; set; }
        public string StrategyType { get; set; }
        public string StrategyName => StrategyType.Substring(StrategyType.LastIndexOf(".") + 1).Replace("Strategy", "");

        public IStrategy GetStrategy()
        {
            var type = Type.GetType(StrategyType);
            return (IStrategy) JsonConvert.DeserializeObject(Strategy, type);
        }

        public SimulationResult SetStrategy(IStrategy strategy)
        {
            StrategyType = strategy.GetType().FullName;
            Strategy = JsonConvert.SerializeObject(strategy);
            return this;
        }
    }
}
