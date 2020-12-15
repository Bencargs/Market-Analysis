using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        StrategyType StrategyType { get; }
        IParameters Parameters { get; }
        void Optimise(DateTime fromDate, DateTime toDate);
        bool ShouldBuy(MarketData data);
    }
}
