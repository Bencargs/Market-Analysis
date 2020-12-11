using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public class TrainingSimulator : ISimulator
    {
        private readonly MarketDataCache _dataCache;
        private readonly SimulationCache _simulationCache;
        private DateTime _latestDate;

        public TrainingSimulator(
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
            ProgressBar _ = null)
        {
            var queue = new OrderQueue();
            foreach (var data in _dataCache.TakeUntil(endDate))
            {
                var latest = _simulationCache.GetOrCreate((strategy, data.Date), previous =>
                {
                    var shouldBuy = ShouldBuy(strategy, data);
                    
                    var state = previous.UpdateState(data, shouldBuy);
                    state.AddFunds(investor);
                    state.ExecuteOrders(queue);

                    if (state.ShouldBuy)
                        state.AddBuyOrder(investor, queue);

                    return state;
                });

                yield return latest;
            }
        }

        private bool ShouldBuy(IStrategy strategy, MarketData data)
        {
            if (data.Date >= _latestDate)
                _latestDate = data.Date;

            return strategy.ShouldBuy(data);
        }
    }
}
