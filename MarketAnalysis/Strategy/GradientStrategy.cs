using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MathNet.Numerics;

namespace MarketAnalysis.Strategy
{
    public class GradientStrategy : IStrategy
    {
        private int _window;
        private decimal _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 524;
        private DateTime _latestDate;

        public object Key => new { _window, _threshold };

        public GradientStrategy(int window, decimal threshold, bool shouldOptimise = true)
        {
            _window = window;
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
            using (var progress = ProgressBarReporter.SpawnChild(10 * 20, "Optimising..."))
            {
                var history = MarketDataCache.Instance.TakeUntil(_latestDate);
                var simulator = new Simulator(history);
                var optimal = Enumerable.Range(1, 10).SelectMany(x =>
                {
                    return Enumerable.Range(20, 20).Select(window =>
                    {
                        var threshold = -((decimal)x / 100);
                        var result = simulator.Evaluate(new GradientStrategy(window, threshold, false)).Last();
                        progress.Tick($"x:{x} y:{window}");
                        return new { threshold, window, result.Worth, result.BuyCount };
                    });
                }).OrderByDescending(x => x.Worth).ThenByDescending(x => x.BuyCount).First();
                _window = optimal.window;
                _threshold = optimal.threshold;
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

            var batch = MarketDataCache.Instance.TakeUntil(_latestDate).ToList().Last(_window);
            if (batch.Length < 2)
                return false;

            var xData = batch.Select(x => (double)x.Price).ToArray();
            var yData = Enumerable.Range(0, batch.Length).Select(x => (double)x).ToArray();
            var parameters = Fit.Line(xData, yData);

            return (parameters.Item2 < (double)_threshold);
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as GradientStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
