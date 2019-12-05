using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Linq;

namespace MarketAnalysis.Simulation
{
    public class BacktestingSimulator : IStimulationStrategy
    {
        private readonly ISimulator _simulator;

        public BacktestingSimulator(ISimulator simulator)
        {
            _simulator = simulator;
        }

        public SimulationState SimulateDay(IStrategy strategy, MarketData data, SimulationState previousState)
        {
            var state = UpdateState(strategy, data, previousState);

            if (strategy.ShouldOptimise())
                Optimise(strategy, data.Date);

            if (strategy.ShouldAddFunds())
                AddFunds(state);

            if (state.ShouldBuy)
                BuyShares(state);

            return state;
        }

        private SimulationState UpdateState(IStrategy strategy, MarketData data, SimulationState previousState)
        {
            var shouldBuy = strategy.ShouldBuyShares(data);

            return new SimulationState
            {
                Date = data.Date,
                SharePrice = data.Price,
                ShouldBuy = shouldBuy,
                Funds = previousState.Funds,
                Shares = previousState.Shares,
                BuyCount = previousState.BuyCount,
            };
        }

        private void BuyShares(SimulationState state)
        {
            var newShares = state.Funds / state.SharePrice;
            state.Shares += newShares;
            state.Funds = 0;
            state.BuyCount++;
        }

        private void AddFunds(SimulationState state)
        {
            // todo: this should be a property of an individual investor
            state.Funds += Configuration.DailyFunds;
        }

        private IStrategy Optimise(IStrategy strategy, DateTime endDate)
        {
            var potentials = strategy.Optimise().ToArray();
            using (var progress = ProgressBarReporter.SpawnChild(potentials.Count(), "Optimising..."))
            {
                var optimal = potentials.Select(strat =>
                {
                    var result = _simulator.Evaluate(strat, endDate, false).Last();
                    progress.Tick();
                    return new { result.Worth, result.BuyCount, strat };
                }).OrderByDescending(x => x.Worth)
                .ThenBy(x => x.BuyCount)
                .FirstOrDefault();
            
                return optimal?.strat ?? strategy;
            }
        }
    }
}
