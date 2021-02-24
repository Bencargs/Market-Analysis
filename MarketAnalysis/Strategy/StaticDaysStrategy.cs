using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy, IEquatable<StaticDatesStrategy>
    {
        private readonly StaticDatesParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.StaticDates;

        public StaticDatesStrategy(StaticDatesParameters parameters)
            => _parameters = parameters;

        public void Optimise(DateTime _, DateTime __) { }

        public bool ShouldBuy(MarketData data)
            => _parameters.BuyDates.TryGetValue(data.Date, out var shouldBuy) && shouldBuy;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(StaticDatesStrategy)) return false;

            return Equals(obj as StaticDatesStrategy);
        }

        public bool Equals(StaticDatesStrategy strategy)
            => strategy._parameters.Identifier == _parameters.Identifier;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Identifier);
    }
}
