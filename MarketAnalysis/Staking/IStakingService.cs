using System;

namespace MarketAnalysis.Staking
{
    public interface IStakingService
    {
        void Evaluate(DateTime fromDate, DateTime toDate);
        decimal GetStake(DateTime today, decimal totalFunds);
    }
}
