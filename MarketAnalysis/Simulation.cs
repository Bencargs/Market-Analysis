using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis
{
    public class Simulation : ISimulation
    {
        private SimulationState _state;
        private Row[] _data;
        private bool _showProgress;

        public int BuyCount => _state.BuyCount;

        public Simulation(IEnumerable<Row> data, bool showProgress = false)
        {
            _data = data.ToArray();
            _showProgress = showProgress;
        }

        public SimulationResult Evaluate(IStrategy strategy)
        {
            using (var progress = InitialiseProgressBar(strategy))
            {
                _state = new SimulationState();
                for (int i = 0; i < _data.Length; i++)
                {
                    var key = Tuple.Create(strategy, i);
                    _state = SimulationCache.GetOrCreate(key, () => SimulateDay(strategy, _data[i], _state));
                    progress?.Tick();
                }
            }

            return GetResults(strategy, _data.LastOrDefault(), _state);
        }

        private ProgressBar InitialiseProgressBar(IStrategy strategy)
        {
            return _showProgress
                ? ProgressBarReporter.StartProgressBar(_data.Count(), strategy.GetType().Name)
                : null;
        }

        private SimulationState SimulateDay(IStrategy strategy, Row day, SimulationState state)
        {
            if (strategy.ShouldOptimise())
                strategy.Optimise();

            if (strategy.ShouldAddFunds())
                AddFunds(state);

            state.LatestPrice = day.Price;
            state.ShouldBuy = strategy.ShouldBuyShares(day);

            if (state.ShouldBuy)
                BuyShares(state);

            return state;
        }

        private SimulationResult GetResults(IStrategy strategy, Row day, SimulationState state)
        {
            var results = new SimulationResult
            {
                Date = day?.Date ?? DateTime.MinValue,
                Worth = state.Funds + (state.Shares * state.LatestPrice),
                BuyCount = BuyCount,
                ShouldBuy = state.ShouldBuy
            };
            results.SetStrategy(strategy);
            return results;
        }

        public void BuyShares(SimulationState state)
        {
            var newShares = state.Funds / state.LatestPrice;
            state.Shares += newShares;
            state.Funds = 0;
            state.BuyCount++;
        }

        private void AddFunds(SimulationState state)
        {
            state.Funds += 10;
        }
    }
}
