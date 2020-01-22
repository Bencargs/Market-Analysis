using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public class Simulator : ISimulator
    {
        private readonly MarketDataCache _dataCache;
        private readonly SimulationCache _simulationCache;
        private readonly ProgressBarProvider _progressProvider;
        private readonly Dictionary<SimulationStatus, IStimulationStrategy> _simulator;

        private enum SimulationStatus
        {
            Training,
            Backtesting,
        }

        public Simulator(MarketDataCache dataCache, SimulationCache simulationCache, ProgressBarProvider progressProvider)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;
            _progressProvider = progressProvider;

            _simulator = new Dictionary<SimulationStatus, IStimulationStrategy>
            {
                {SimulationStatus.Training, new TrainingSimulator() },
                {SimulationStatus.Backtesting, new BacktestingSimulator(this, _progressProvider) }
            };
        }

        public IEnumerable<SimulationState> Evaluate(IStrategy strategy, DateTime? endDate = null, ProgressBar parentProgress = null)
        {
            using (var progress = InitialiseProgressBar(strategy.StrategyType, parentProgress))
            {
                foreach (var data in _dataCache.TakeUntil(endDate))
                {
                    var key = (strategy, data.Date);
                    var latest = _simulationCache.GetOrCreate(key, prev =>
                    {
                        var state = GetSimulationState(data.Date);
                        return _simulator[state].SimulateDay(strategy, data, prev, progress);
                    });

                    progress?.Tick();
                    yield return latest;
                }
            }
        }

        private SimulationStatus GetSimulationState(DateTime date)
        {
            var state = SimulationStatus.Training;
            if (date > Configuration.BacktestingDate)
                state = SimulationStatus.Backtesting;

            return state;
        }

        private ChildProgressBar InitialiseProgressBar(StrategyType strategy, ProgressBar parentProgress)
        {
            return parentProgress != null
                ? _progressProvider.Create(parentProgress, _dataCache.Count, strategy.GetDescription())
                : null;
        }

        public void RemoveCache(IEnumerable<IStrategy> strategies)
        {
            if (_simulationCache.Count < Configuration.CacheSize)
                return;

            foreach (var strategy in strategies)
                _simulationCache.Remove(strategy);
        }
    }
}
