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
        private MarketDataCache _dataCache;
        private SimulationCache _simulationCache;
        private Dictionary<SimulationStatus, StimulationStrategy> _simulator;

        private enum SimulationStatus
        {
            Training,
            Backtesting,
        }

        public Simulator(MarketDataCache dataCache, SimulationCache simulationCache)
        {
            _dataCache = dataCache;
            _simulationCache = simulationCache;

            _simulator = new Dictionary<SimulationStatus, StimulationStrategy>
            {
                {SimulationStatus.Training, new TrainingSimulator(simulationCache) },
                {SimulationStatus.Backtesting, new BacktestingSimulator(simulationCache, this) }
            };
        }

        public List<SimulationState> Evaluate(IStrategy strategy, DateTime? endDate = null)
        {
            var history = new List<SimulationState>();
            using (var progress = InitialiseProgressBar(strategy))
            {
                foreach (var d in _dataCache.TakeUntil(endDate))
                {
                    var date = d.Date;
                    var key = (strategy, date);
                    var latestState = _simulationCache.GetOrCreate(key, () =>
                    {
                        var state = GetSimulationState(date);
                        return _simulator[state].SimulateDay(strategy, d);
                    });
                    history.Add(latestState);

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

        private ProgressBar InitialiseProgressBar(IStrategy strategy)
        {
            //return _showProgress
            //    ? ProgressBarReporter.StartProgressBar(_data.Count(), strategy.GetType().Name)
            //    : null;
            return null;
        }
    }
}
