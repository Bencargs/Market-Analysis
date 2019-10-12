using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : IStrategy
    {
        private int _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 524;
        private List<Row> _history = new List<Row>(5000);

        public object Key => _threshold;

        public VolumeStrategy(int threshold, bool shouldOptimise = true)
        {
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
            using (var progress = ProgressBarReporter.SpawnChild(800, "Optimising..."))
            {
                var simulator = new Simulation(_history);
                var optimal = Enumerable.Range(1, 800).Select(x =>
                {
                    var result = simulator.Evaluate(new VolumeStrategy(x, false));
                    progress.Tick($"Optimising... x:{x}");
                    return new { x, result.Worth, simulator.BuyCount };
                }).OrderByDescending(x => x.Worth).ThenBy(x => x.BuyCount).First();
                _threshold = optimal.x;
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

            return data.Volume < _threshold;
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as VolumeStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
