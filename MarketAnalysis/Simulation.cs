﻿using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis
{
    public class Simulation : ISimulation
    {
        private decimal _funds;
        private decimal _shares;
        private bool _shouldBuy;
        private decimal _latestPrice;
        private bool _isSelfOptimising;
        private List<Row> _data = new List<Row>();

        public int BuyCount { get; private set; }

        public Simulation(List<Row> data, bool shouldOptimise = true)
        {
            _data = data;
            _isSelfOptimising = shouldOptimise;
        }

        public SimulationResult Evaluate(IStrategy strategy)
        {
            Reset();
            for (int i = 0; i < _data.Count; i++)
            {
                if (_isSelfOptimising && i % Configuration.OptimisePeriod == 0)
                    strategy.Optimise();

                if (strategy.ShouldAddFunds())
                    AddFunds();

                _latestPrice = _data[i].Price;
                _shouldBuy = strategy.ShouldBuyShares(_data[i]);
                if (_shouldBuy)
                    BuyShares();
            }
            return GetResults(strategy);
        }

        private void Reset()
        {
            _funds = 0;
            _shares = 0;
            _shouldBuy = false;
            _latestPrice = 0;

            BuyCount = 0;
        }

        private SimulationResult GetResults(IStrategy strategy)
        {
            var results = new SimulationResult
            {
                Date = _data.Last().Date,
                Worth = _funds + (_shares * _latestPrice),
                BuyCount = BuyCount,
                ShouldBuy = _shouldBuy
            };
            results.SetStrategy(strategy);
            return results;
        }
        
        private void AddFunds()
        {
            _funds += 10;
        }

        private void BuyShares()
        {
            var newShares = _funds / _latestPrice;
            _shares += newShares;
            _funds = 0;

            BuyCount++;
        }
    }
}
