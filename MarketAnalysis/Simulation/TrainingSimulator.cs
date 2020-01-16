using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;

namespace MarketAnalysis.Simulation
{
    public class TrainingSimulator : IStimulationStrategy
    {
        public SimulationState SimulateDay(IStrategy _, MarketData data, SimulationState previousState, ChildProgressBar __)
        {
            var state = UpdateState(data, previousState);

            return state;
        }

        private SimulationState UpdateState(MarketData data, SimulationState previousState)
        {
            return new SimulationState
            {
                Date = data.Date,
                SharePrice = data.Price,
                ShouldBuy = false,
                Funds = previousState.Funds,
                Shares = previousState.Shares,
                BuyCount = previousState.BuyCount,
            };
        }
    }
}
