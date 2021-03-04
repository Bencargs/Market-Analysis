using System;
using System.Collections.Generic;
using MarketAnalysis.Models;

namespace MarketAnalysis.Strategy.Parameters
{
    public class ClusteringParameters : IParameters
    {
        public decimal Threshold { get; set; }
        public int Partitions { get; set; } = 5;
        public Grid<List<decimal>> Grid { get; set; } = new();
        public TimeSpan? OptimisePeriod { get; } = TimeSpan.FromDays(1024);
    }
}
