using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : IStrategy
    {
        private readonly int _window;
        private static TimeSpan OptimisePeriod = TimeSpan.FromDays(1024);
        private DateTime _latestDate;
        private DateTime? _lastOptimised;

        public object Key => _window;

        public LinearRegressionStrategy(int window, bool shouldOptimise = true)
        {
            _window = window;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?)null;
        }

        public bool ShouldOptimise()
        {
            if (_lastOptimised != null &&
                _latestDate > (_lastOptimised + OptimisePeriod))
            {
                _lastOptimised = _latestDate;
                return true;
            }
            return false;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return Enumerable.Range(30, 200).Select(x => 
                new LinearRegressionStrategy(x, false));
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            var latestPoints = MarketDataCache.Instance.GetLastSince(_latestDate, _window)
                .Select((x, i) => new XYPoint { X = i, Y = x.Price }).ToArray();
            if (latestPoints.Length < 2)
                return false;

            GenerateLinearBestFit(latestPoints, out double m, out double b);
            var prediction = (decimal) (m * MarketDataCache.Instance.Count - b);
            return (data.Price < prediction);
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
