using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : IStrategy
    {
        private int _window;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private DateTime _latestDate;

        public object Key => _window;

        public LinearRegressionStrategy(int window, bool shouldOptimise = true)
        {
            _window = window;
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
                var optimal = Enumerable.Range(30, 200).Select(x =>
                {
                    var result = simulator.Evaluate(new LinearRegressionStrategy(x, false)).Last();
                    progress.Tick($"Optimising... x:{x}");
                    return new { x, result.Worth, result.BuyCount };
                }).OrderByDescending(x => x.Worth).ThenBy(x => x.BuyCount).First();
                _window = optimal.x;
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

            var latestPoints = MarketDataCache.Instance.GetLastSince(_latestDate, _window)
                .Select((x, i) => new XYPoint { X = i, Y = x.Price }).ToArray();
            if (latestPoints.Length < 2)
                return false;

            GenerateLinearBestFit(latestPoints, out double m, out double b);
            var prediction = (decimal) (m * MarketDataCache.Instance.Count - b);
            return (data.Price > prediction);
        }

        private void GenerateLinearBestFit(XYPoint[] points, out double m, out double b)
        {
            int numPoints = points.Length;
            double meanX = points.Average(point => point.X);
            decimal meanY = points.Average(point => point.Y);

            double sumXSquared = Math.Pow(points.Sum(x => x.X), 2);
            decimal sumXY = points.Sum(point => point.X * point.Y);

            var meanSqrd = Math.Pow(meanX, 2);
            var back = (sumXSquared / numPoints - meanSqrd);
            var mid = numPoints - meanX * ((double)meanY);
            var front = ((double)sumXY);

            m = (front / mid) / back;
            b = (m * meanX - ((double)meanY));
        }

        private class XYPoint
        {
            public int X;
            public decimal Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as LinearRegressionStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
