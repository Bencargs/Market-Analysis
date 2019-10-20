using System.Collections.Generic;
using System.Linq;
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
        private List<Row> _history = new List<Row>(5000);

        public object Key => new { _window, _threshold };

        public GradientStrategy(int window, decimal threshold, bool shouldOptimise = true)
        {
            _window = window;
            _threshold = threshold;
            _shouldOptimise = shouldOptimise;
        }

        public bool ShouldOptimise()
        {
            return _shouldOptimise &&
                   _history.Count > 1 &&
                   _history.Count % OptimisePeriod == 0;
        }

        public void Optimise()
        {
            using (var progress = ProgressBarReporter.SpawnChild(10 * 20, "Optimising..."))
            {
                var simulator = new Simulator(_history);
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

        public bool ShouldBuyShares(Row data)
        {
            if (!_history.Any(x => x.Date == data.Date))
                _history.Add(data);

            var batch = _history.AsEnumerable().Reverse().Take(_window).Reverse().ToArray();
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
