using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis
{
    public class Simulator : ISimulator
    {
        private MarketData[] _data;
        private bool _showProgress;

        public Simulator(IEnumerable<MarketData> data, bool showProgress = false)
        {
            _data = data.ToArray();
            _showProgress = showProgress;
        }

        public List<SimulationState> Evaluate(IStrategy strategy)
        {
            var history = new List<SimulationState>();
            using (var progress = InitialiseProgressBar(strategy))
            {
                for (int i = 0; i < _data.Length; i++)
                {
                    var key = (strategy, i);
                    var previousState = GetPreviousState(history);
                    var latestState = SimulationCache.Instance.GetOrCreate(key, () => SimulateDay(strategy, _data[i], previousState));
                    history.Add(latestState);
                    progress?.Tick();
                }
            }
            return history;
        }

        private SimulationState GetPreviousState(List<SimulationState> history)
        {
            var previousState = history.LastOrDefault() ?? new SimulationState();
            return new SimulationState
            {
                Date = previousState.Date,
                Funds = previousState.Funds,
                Shares = previousState.Shares,
                ShouldBuy = previousState.ShouldBuy,
                SharePrice = previousState.SharePrice,
                BuyCount = previousState.BuyCount
            };
        }

        private ProgressBar InitialiseProgressBar(IStrategy strategy)
        {
            return _showProgress
                ? ProgressBarReporter.StartProgressBar(_data.Count(), strategy.GetType().Name)
                : null;
        }

        private SimulationState SimulateDay(IStrategy strategy, MarketData day, SimulationState state)
        {
            if (strategy.ShouldOptimise())
                strategy.Optimise();

            if (strategy.ShouldAddFunds())
                AddFunds(state);

            state.Date = day.Date;
            state.SharePrice = day.Price;
            state.ShouldBuy = strategy.ShouldBuyShares(day);

            if (state.ShouldBuy)
                BuyShares(state);

            return state;
        }

        public void BuyShares(SimulationState state)
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
    }
}
