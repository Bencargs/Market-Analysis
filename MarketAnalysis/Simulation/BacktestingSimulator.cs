using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Simulation
{
    public class BacktestingSimulator : IStimulationStrategy
    {
        private readonly ISimulator _simulator;
        private readonly ProgressBarProvider _progressProvider;

        public BacktestingSimulator(ISimulator simulator, ProgressBarProvider progressProvider)
        {
            _simulator = simulator;
            _progressProvider = progressProvider;
        }

        public SimulationState SimulateDay(IStrategy strategy, MarketData data, SimulationState previousState, ChildProgressBar progress)
        {
            var state = UpdateState(strategy, data, previousState);

            AddFunds(state);

            if (strategy.ShouldOptimise())
                Optimise(strategy, data.Date, progress);

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

        private void Optimise(IStrategy strategy, DateTime endDate, ChildProgressBar progress)
        {
            var potentials = strategy.GetOptimisations();
            
            var optimal = FindOptimum(potentials, endDate, progress);

            ClearCache(optimal, potentials);

            strategy.SetParameters(optimal);
        }

        private void ClearCache(IStrategy optimal, IEnumerable<IStrategy> potentials)
        {
            if (optimal is IAggregateStrategy)
            {
                var redundantStrategies = potentials.Except(new[] { optimal });
                _simulator.RemoveCache(redundantStrategies);
            }
        }

        private IStrategy FindOptimum(IEnumerable<IStrategy> potentials, DateTime endDate, ChildProgressBar progress)
        {
            using (var childProgress = _progressProvider.Create(progress, potentials.Count(), "Optimising..."))
            {
                return potentials.Select(strat =>
                {
                    var result = _simulator.Evaluate(strat, endDate).Last();
                    childProgress?.Tick();
                    return ( result.Worth, result.BuyCount, strat );
                })
                .OrderByDescending(x => x.Worth)
                .ThenBy(x => x.BuyCount)
                .First().strat;
            }
        }
    }
}
