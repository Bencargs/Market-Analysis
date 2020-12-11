using MarketAnalysis.Models;
using MarketAnalysis.Strategy.Parameters;
using System;

namespace MarketAnalysis.Strategy
{
    public interface IStrategy
    {
        StrategyType StrategyType { get; }
        IParameters Parameters { get; }
        void Optimise(DateTime date);
        bool ShouldBuy(MarketData data);
    }
}
