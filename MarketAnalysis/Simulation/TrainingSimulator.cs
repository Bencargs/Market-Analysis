﻿using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Simulation
{
    public class TrainingSimulator : ISimulator
    {
        private readonly IMarketDataCache _dataCache;
        private readonly ISimulationCache _simulationCache;
        private DateTime _latestDate;

        public TrainingSimulator(
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
            ProgressBar _ = null)
        {
            var state = new SimulationState();
            foreach (var data in _dataCache.TakeUntil(endDate))
            {
                var shouldBuy = _simulationCache.GetOrCreate((strategy, data.Date), () => ShouldBuy(strategy, data));

                state = state.UpdateState(data, shouldBuy);
                state.AddFunds(investor.DailyFunds);
                state.ExecuteOrders();

                if (state.ShouldBuy)
                {
                    var funds = strategy.GetStake(data.Date, state.TotalFunds);
                    state.AddBuyOrder(investor.OrderBrokerage, investor.OrderDelayDays, funds);
                }

                yield return state;
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
