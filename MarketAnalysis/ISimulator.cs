using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public interface ISimulator
    {
        List<SimulationState> Evaluate(IStrategy strategy);
    }
}
