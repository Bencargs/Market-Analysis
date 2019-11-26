using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;

namespace MarketAnalysis.Simulation
{
    public class TrainingSimulator : StimulationStrategy
    {
        public TrainingSimulator(SimulationCache cache)
            : base(cache)
        {
        }

        public override SimulationState SimulateDay(IStrategy strategy, MarketData data)
        {
            var previousState = GetPreviousState(strategy, data);
            var state = UpdateState(strategy, data, previousState);

            return state;
        }
    }
}
