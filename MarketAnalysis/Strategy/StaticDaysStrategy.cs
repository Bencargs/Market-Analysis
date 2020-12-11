using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy
    {
        private StaticDatesParameters _parameters;

        public IParameters Parameters 
        {
            get => _parameters;
            private set => _parameters = (StaticDatesParameters)value; 
        }
        public StrategyType StrategyType { get; } = StrategyType.StaticDates;

        public StaticDatesStrategy(StaticDatesParameters parameters)
        {
            Parameters = parameters;
        }

        public void Optimise(DateTime latestDate) { }

        public bool ShouldBuy(MarketData data)
        {
            return _parameters.BuyDates[data.Date];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StaticDatesStrategy strategy))
                return false;

            return strategy._parameters.Identifier == _parameters.Identifier;
        }

        public override int GetHashCode()
        {
            return _parameters.Identifier;
        }
    }
}
