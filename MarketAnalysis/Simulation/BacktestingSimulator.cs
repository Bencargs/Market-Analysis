using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Linq;

namespace MarketAnalysis.Simulation
{
    public class BacktestingSimulator : StimulationStrategy
    {
        private readonly ISimulator _simulator;

        public BacktestingSimulator(SimulationCache cache, ISimulator simulator)
            : base(cache)
        {
            _simulator = simulator;
        }

        public override SimulationState SimulateDay(IStrategy strategy, MarketData data)
        {
            var previousState = GetPreviousState(strategy, data);
            var state = UpdateState(strategy, data, previousState);

            if (strategy.ShouldOptimise())
                Optimise(strategy, data.Date);

            if (strategy.ShouldAddFunds())
                AddFunds(state);

            if (state.ShouldBuy)
                BuyShares(state);

            return state;
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
            var potentials = strategy.Optimise();
            var optimal = potentials.Select(strat =>
            {
                var result = _simulator.Evaluate(strat, endDate).Last();
                return new { result.Worth, result.BuyCount, strat };
            }).OrderByDescending(x => x.Worth).ThenBy(x => x.BuyCount)
            .FirstOrDefault();

            return optimal?.strat ?? strategy;
        }
    }
}
