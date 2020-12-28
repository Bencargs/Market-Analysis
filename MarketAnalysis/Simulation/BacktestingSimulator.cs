using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public class BacktestingSimulator : ISimulator
    {
        private readonly IMarketDataCache _dataCache;
        private readonly ISimulationCache _simulationCache;
        private DateTime _latestDate;
        private DateTime _lastOptimised;

        public BacktestingSimulator(
            IMarketDataCache dataCache,
            ISimulationCache simulationCache)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;
        }

        public IEnumerable<SimulationState> Evaluate(
            IStrategy strategy,
            Investor investor,
            DateTime? endDate = null,
            ProgressBar progress = null)
        {
            var queue = new OrderQueue();
            var state = new SimulationState();
            var backtestingDate = Configuration.BacktestingDate;
            var trainer = new TrainingSimulator(_dataCache, _simulationCache);
            foreach (var s in trainer.Evaluate(strategy, investor, backtestingDate, progress))
            {
                yield return state = s;
            }

            var remaining = _dataCache.Count - _dataCache.BacktestingIndex;
            using var childProgress = ProgressBarProvider.Create(progress, remaining, $"Evaluating: {strategy.StrategyType.GetDescription()}");
            foreach (var data in _dataCache.TakeFrom(backtestingDate, endDate))
            {
                Optimise(strategy);
                var shouldBuy = _simulationCache.GetOrCreate((strategy, data.Date), () => ShouldBuy(strategy, data));

                state = state.UpdateState(data, shouldBuy);
                state.AddFunds(investor);
                state.ExecuteOrders(queue);

                if (state.ShouldBuy)
                    state.AddBuyOrder(investor, queue);

                childProgress?.Tick();
                yield return state;
            }
        }

        private void Optimise(IStrategy strategy)
        {
            if (!strategy.Parameters.OptimisePeriod.HasValue)
                return;

            var nextOptimisation = _lastOptimised + strategy.Parameters.OptimisePeriod;
            if (nextOptimisation > _latestDate)
                return;

            strategy.Optimise(_lastOptimised, _latestDate);

            _lastOptimised = _latestDate;
        }

        private bool ShouldBuy(IStrategy strategy, MarketData data)
        {
            if (data.Date >= _latestDate)
                _latestDate = data.Date;

            return strategy.ShouldBuy(data);
        }
    }
}
