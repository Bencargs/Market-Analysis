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
        private readonly MarketDataCache _dataCache;
        private readonly SimulationCache _simulationCache;
        private DateTime _latestDate;
        private DateTime _lastOptimised;

        public BacktestingSimulator(
            MarketDataCache dataCache,
            SimulationCache simulationCache)
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
            using var childProgress = ProgressBarProvider.Create(progress, _dataCache.Count, $"Evaluating: {strategy.StrategyType.GetDescription()}");

            SimulationState latest = null;
            var backtestingDate = Configuration.BacktestingDate;
            var trainer = new TrainingSimulator(_dataCache, _simulationCache);
            foreach (var state in trainer.Evaluate(strategy, investor, backtestingDate, progress))
            {
                latest = state;

                childProgress?.Tick();
                yield return latest;
            }

            foreach (var data in _dataCache.TakeFrom(backtestingDate, endDate))
            {
                latest =_simulationCache.GetOrCreate((strategy, data.Date), previous =>
                {
                    if (ShouldOptimise(strategy, previous))
                        strategy.Optimise(_latestDate);
                    var shouldBuy = ShouldBuy(strategy, data);

                    var state = previous.UpdateState(data, shouldBuy);
                    state.AddFunds(investor);
                    state.ExecuteOrders(queue);

                    if (state.ShouldBuy)
                        state.AddBuyOrder(investor, queue);

                    return state;
                });

                childProgress?.Tick();
                yield return latest;
            }
        }

        private bool ShouldOptimise(IStrategy strategy, SimulationState state)
        {
            var nextOptimisation = _lastOptimised + strategy.Parameters.OptimisePeriod;
            if (nextOptimisation > _latestDate)
                return false;

            _lastOptimised = _latestDate;
            return true;
        }

        private bool ShouldBuy(IStrategy strategy, MarketData data)
        {
            if (data.Date >= _latestDate)
                _latestDate = data.Date;

            return strategy.ShouldBuy(data);
        }
    }
}
