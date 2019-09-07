using MarketAnalysis.Strategy;
using Newtonsoft.Json;
using System;

namespace MarketAnalysis.Models
{
    public class SimulationResult
    {
        public DateTime Date { get; set; }
        public decimal Worth { get; set; }
        public int BuyCount { get; set; }
        public bool ShouldBuy { get; set; }
        public string Strategy { get; set; }
        public string StrategyType { get; set; }

        public IStrategy GetStrategy()
        {
            var type = Type.GetType(StrategyType);
            return (IStrategy) JsonConvert.DeserializeObject(Strategy, type);
        }

        public void SetStrategy(IStrategy strategy)
        {
            StrategyType = strategy.GetType().FullName;
            Strategy = JsonConvert.SerializeObject(strategy);
        }
    }
}
