using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;
using MarketAnalysis.Staking;
using Nager.Date;

namespace MarketAnalysis.Strategy
{
    public class HolidayEffectStrategy : IStrategy, IEquatable<HolidayEffectStrategy>
    {
        private readonly HolidayEffectParameters _parameters;
        private readonly IStakingService _stakingService;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.HolidayEffect;

        public HolidayEffectStrategy(
            HolidayEffectParameters parameters, IStakingService stakingService)
        {
            _parameters = parameters;
            _stakingService = stakingService;
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            _stakingService.Evaluate(fromDate, toDate);
        }

        public bool ShouldBuy(MarketData data)
            => DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.AU) ||
               DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.US) ||
               DateSystem.IsPublicHoliday(data.Date.AddDays(1), CountryCode.CN);

        public decimal GetStake(decimal totalFunds)
        {
            return _stakingService.GetStake(totalFunds);
        }

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
