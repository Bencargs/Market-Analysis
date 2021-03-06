﻿using System;

namespace MarketAnalysis.Models
{
    public class MarketData
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Delta { get; set; }
        public decimal Volume { get; set; }
        public decimal Spread { get; set; }
        public decimal DeltaPercent { get; set; }
        public decimal VolumePercent { get; set; }
        public decimal SpreadPercent { get; set; }
    }
}
