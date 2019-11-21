using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy
    {
        private decimal _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private DateTime _latestDate;

        public object Key => _threshold;

        public DeltaStrategy(decimal threshold, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _shouldOptimise = shouldOptimise;
        }

        public bool ShouldOptimise()
        {
            var count = MarketDataCache.Instance.Count;
            return _shouldOptimise &&
                   count > 1 &&
                   count % OptimisePeriod == 0;
        }

        public void Optimise()
        {
            using (var progress = ProgressBarReporter.SpawnChild(200, "Optimising..."))
            {
                var history = MarketDataCache.Instance.TakeUntil(_latestDate);
                var simulator = new Simulator(history);
                var optimal = Enumerable.Range(1, 200).Select(x =>
                {
                    var parameter = (decimal) x / 1000;
                    var result = simulator.Evaluate(new DeltaStrategy(parameter, false)).Last();
                    progress.Tick($"Optimising... x:{x}");
                    return new { parameter, result.Worth, result.BuyCount };
                }).OrderByDescending(x => x.Worth).First();
                _threshold = optimal.parameter;
            }
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (MarketDataCache.Instance.TryAdd(data))
                _latestDate = data.Date;

            return Math.Abs(data.Delta) < _threshold;
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as DeltaStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
