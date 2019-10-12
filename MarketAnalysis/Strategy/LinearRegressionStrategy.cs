using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : IStrategy
    {
        private int _window;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
        private List<Row> _history = new List<Row>(5000);

        public object Key => _window;

        public LinearRegressionStrategy(int window, bool shouldOptimise = true)
        {
            _window = window;
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
            using (var progress = ProgressBarReporter.SpawnChild(200, "Optimising..."))
            {
                var simulator = new Simulation(_history);
                var optimal = Enumerable.Range(30, 200).Select(x =>
                {
                    var result = simulator.Evaluate(new LinearRegressionStrategy(x, false));
                    progress.Tick($"Optimising... x:{x}");
                    return new { x, result.Worth, simulator.BuyCount };
                }).OrderByDescending(x => x.Worth).ThenBy(x => x.BuyCount).First();
                _window = optimal.x;
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

            var latestPoints = _history.AsEnumerable().Reverse().Take(_window).Reverse()
                .Select((x, i) => new XYPoint { X = i, Y = x.Price }).ToList();
            if (latestPoints.Count < 2)
                return false;

            GenerateLinearBestFit(latestPoints, out double m, out double b);
            var prediction = (decimal) (m * _history.Count - b);
            return (data.Price > prediction);
        }

        private void GenerateLinearBestFit(List<XYPoint> points, out double m, out double b)
        {
            int numPoints = points.Count;
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
