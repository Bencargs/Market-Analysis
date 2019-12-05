using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public interface ISimulator
    {
        IEnumerable<SimulationState> Evaluate(IStrategy strategy, DateTime? endDate = null, bool showProgress = true);
    }
}
