﻿using MarketAnalysis.Models;
using MarketAnalysis.Strategy;

namespace MarketAnalysis
{
    public class ConstantStrategy : IStrategy
    {
        public void Optimise()
        {
            return;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            return true;
        }
    }
}
