using MarketAnalysis.Caching;
using MarketAnalysis.Models;
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
        private readonly Dictionary<SimulationStatus, IStimulationStrategy> _simulator;

        private enum SimulationStatus
        {
            Training,
            Backtesting,
        }

        public Simulator(MarketDataCache dataCache, SimulationCache simulationCache)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;

            _simulator = new Dictionary<SimulationStatus, IStimulationStrategy>
            {
                {SimulationStatus.Training, new TrainingSimulator() },
                {SimulationStatus.Backtesting, new BacktestingSimulator(this) }
            };
        }

        public IEnumerable<SimulationState> Evaluate(IStrategy strategy, DateTime? endDate = null, bool showProgress = true)
        {
            var history = new List<SimulationState>(5000);
            using (var progress = InitialiseProgressBar(strategy.StrategyType, showProgress))
            {
                foreach (var data in _dataCache.TakeUntil(endDate))
                {
                    var key = (strategy, data.Date);
                    var latest = _simulationCache.GetOrCreate(key, prev =>
                    {
                        var state = GetSimulationState(data.Date);
                        return _simulator[state].SimulateDay(strategy, data, prev);
                    });
                    history.Add(latest);

                    progress?.Tick();
                }
            }
            return history;
        }

        private SimulationStatus GetSimulationState(DateTime date)
        {
            var state = SimulationStatus.Training;
            if (date > Configuration.BacktestingDate)
                state = SimulationStatus.Backtesting;

            return state;
        }

        private ProgressBar InitialiseProgressBar(StrategyType strategy, bool showProgress)
        {
            return showProgress
                ? ProgressBarReporter.StartProgressBar(_dataCache.Count, strategy.GetDescription())
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
