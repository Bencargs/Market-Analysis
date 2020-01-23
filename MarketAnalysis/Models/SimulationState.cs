﻿using System;

namespace MarketAnalysis.Models
{
    public class SimulationState
    {
        public DateTime Date { get; set; }
        public decimal Funds { get; set; }
        public decimal Shares { get; set; }
        public bool ShouldBuy { get; set; }
        public decimal SharePrice { get; set; }
        public int BuyCount { get; set; }
        public decimal Worth => Funds + (Shares * SharePrice);

        public override bool Equals(object obj)
        {
            if (!(obj is SimulationState other))
                return false;

            return Date == other.Date &&
                   Funds == other.Funds &&
                   Shares == other.Shares &&
                   ShouldBuy == other.ShouldBuy &&
                   BuyCount == other.BuyCount;
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode() ^
                   Funds.GetHashCode() ^
                   Shares.GetHashCode() ^
                   ShouldBuy.GetHashCode() ^
                   BuyCount.GetHashCode();
        }
    }
}
