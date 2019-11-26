using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public interface ISimulator
    {
        List<SimulationState> Evaluate(IStrategy strategy, DateTime? endDate = null);
    }
}
