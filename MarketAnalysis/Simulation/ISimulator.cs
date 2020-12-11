using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public interface ISimulator
    {
        IEnumerable<SimulationState> Evaluate(
            IStrategy strategy, 
            Investor investor, 
            DateTime? endDate = null, 
            ProgressBar progress = null);
        //void RemoveCache(IEnumerable<IStrategy> strategies);
    }
}
