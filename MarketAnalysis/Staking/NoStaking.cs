using System;

namespace MarketAnalysis.Staking
{
    public class NoStaking : IStakingService
    {
        public void Evaluate(DateTime _, DateTime __)
        {
        }

        public decimal GetStake(decimal totalFunds) => totalFunds;
    }
}
