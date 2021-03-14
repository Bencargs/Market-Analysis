using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy
    {
        private readonly StaticDatesParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.StaticDates;

        public StaticDatesStrategy(StaticDatesParameters parameters)
            => _parameters = parameters;

        public void Optimise(DateTime _, DateTime __) { }

        public bool ShouldBuy(MarketData data)
            => _parameters.BuyDates.TryGetValue(data.Date, out var shouldBuy) && shouldBuy;
    }
}
