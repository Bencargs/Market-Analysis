using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis
{
    public class Simulation : ISimulation
    {
        private SimulationState _state;
        private Row[] _data;

        public int BuyCount => _state.BuyCount;

        public Simulation(IEnumerable<Row> data)
        {
            _data = data.ToArray();
        }

        public SimulationResult Evaluate(IStrategy strategy)
        {
            _state = new SimulationState();
            for (int i = 0; i < _data.Length; i++)
            {
                var key = Tuple.Create(strategy, i);
                _state = SimulationCache.GetOrAdd(key, () => SimulateDay(strategy, _data[i], _state));
            }
            return GetResults(strategy, _data.LastOrDefault(), _state);
        }

        private SimulationState SimulateDay(IStrategy strategy, Row day, SimulationState state)
        {
            if (strategy.ShouldOptimise())
                strategy.Optimise();

            if (strategy.ShouldAddFunds())
                AddFunds(state);

            state.LatestPrice = day.Price;

            if (strategy.ShouldBuyShares(day))
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
            state.ShouldBuy = true;
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
