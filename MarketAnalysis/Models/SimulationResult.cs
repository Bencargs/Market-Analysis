﻿using System;
using System.Collections.Generic;

namespace MarketAnalysis.Models
{
    public class SimulationResult
    {
        public DateTime Date { get; set; }
        public Investor Investor { get; set; }
        public int SimulationDays { get; set; }
        public decimal ProfitYTD { get; set; }
        public decimal ProfitTotal { get; set; }
        public decimal AboveMarketReturn { get; set; }
        public decimal Alpha { get; set; }
        public decimal MaximumAlpha { get; set; }
        public decimal MaximumDrawdown { get; set; }
        public int BuyCount { get; set; }
        public int MaximumHoldingPeriod { get; set; }
        public decimal SharpeRatio { get; set; }
        public double MarketCorrelation { get; set; }
        public decimal Accuracy { get; set; }
        public decimal Recall { get; set; }
        public decimal Precision { get; set; }
        public Dictionary<ConfusionCategory, int> ConfusionMatrix { get; set; }
        public decimal AverageReturn { get; set; }
        public decimal[] History { get; set; }
        public bool ShouldBuy { get; set; }
        public decimal Stake { get; set; }
        public decimal TotalFunds { get; set; }
        public string StrategyType { get; set; }
    }
}
