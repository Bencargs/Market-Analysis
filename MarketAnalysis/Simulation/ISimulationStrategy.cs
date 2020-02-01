using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;

namespace MarketAnalysis.Simulation
{
    public interface IStimulationStrategy
    {
        SimulationState SimulateDay(
            IStrategy strategy, 
            MarketData data,
            OrderQueue queue,
            SimulationState previousState, 
            ChildProgressBar progress);
    }
}
