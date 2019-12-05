using MarketAnalysis.Models;
using MarketAnalysis.Strategy;

namespace MarketAnalysis.Simulation
{
    public interface IStimulationStrategy
    {
        SimulationState SimulateDay(IStrategy strategy, MarketData data, SimulationState previousState);
    }
}
