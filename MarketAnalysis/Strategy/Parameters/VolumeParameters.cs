using System;

namespace MarketAnalysis.Strategy.Parameters
{
    public class VolumeParameters : IParameters
    {
        public decimal PreviousVolume { get; set; }
        public int Threshold { get; set; }
        public TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(256);
    }
}
