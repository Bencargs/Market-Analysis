using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;
using Nager.Date;

namespace MarketAnalysis.Strategy
{
    public class HolidayEffectStrategy : IStrategy, IEquatable<HolidayEffectStrategy>
    {
        private readonly HolidayEffectParameters _parameters;
        
        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.HolidayEffect;

        public HolidayEffectStrategy(
            HolidayEffectParameters parameters)
            => _parameters = parameters;

        public void Optimise(DateTime fromDate, DateTime toDate) { }

        public bool ShouldBuy(MarketData data)
            => DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.AU) ||
               DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.US) ||
               DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.CN);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(HolidayEffectStrategy)) return false;

            return Equals(obj as HolidayEffectStrategy);
        }

        public bool Equals(HolidayEffectStrategy other)
            => true;

        public override int GetHashCode()
            => HashCode.Combine(_parameters);
    }
}
