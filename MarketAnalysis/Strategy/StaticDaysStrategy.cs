using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy, IEquatable<StaticDatesStrategy>
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

        public void Optimise(DateTime _, DateTime __) { }

        public bool ShouldBuy(MarketData data)
        {
            return _parameters.BuyDates[data.Date];
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(StaticDatesStrategy)) return false;

            return Equals(obj as StaticDatesStrategy);
        }

        public bool Equals(StaticDatesStrategy strategy)
        {
            return strategy._parameters.Identifier == _parameters.Identifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Identifier);
        }
    }
}
