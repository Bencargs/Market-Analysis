using System;

namespace MarketAnalysis.Staking
{
    public class DollarCostAveraging : IStakingService
    {
        public void Evaluate(DateTime _, DateTime __)
        {
        }

        public decimal GetStake(DateTime _, decimal totalFunds) => totalFunds;
    }
}
