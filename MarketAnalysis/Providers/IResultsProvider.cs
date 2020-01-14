﻿using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public interface IResultsProvider
    {
        void Initialise();
        void AddResults(IStrategy strategy, IEnumerable<SimulationState> history);
        IEnumerable<SimulationResult> GetResults();
        Task SaveSimulationResults();
        Task SaveData(Task<IEnumerable<MarketData>> data);
        bool ShouldBuy();
        decimal TotalProfit();
    }
}
