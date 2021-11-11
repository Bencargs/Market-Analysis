using System;

namespace MarketAnalysis.Staking
{
    public class StaticFractionalStaking : IStakingService
    {
        private readonly decimal _fraction;

        public StaticFractionalStaking(decimal fraction)
        {
            _fraction = fraction;
        }

        public void Evaluate(DateTime _, DateTime __)
        {
        }

        public decimal GetStake(DateTime _, decimal totalFunds)
            => totalFunds * _fraction;
    }
}
