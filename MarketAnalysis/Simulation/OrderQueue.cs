﻿using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Simulation
{
    public class OrderQueue
    {
        private readonly Queue<MarketOrder> _orders = new();

        public void Add(MarketOrder order)
        {
            _orders.Enqueue(order);
        }

        public IEnumerable<MarketOrder> Get(DateTime date)
        {
            while (ShouldEnumerate(date))
            {
                yield return _orders.Dequeue();
            }
        }

        public decimal Worth()
            => _orders.Sum(x => x.Funds);

        private bool ShouldEnumerate(DateTime date) => _orders.Any() && _orders.Peek().ExecutionDate <= date;
    }
}
