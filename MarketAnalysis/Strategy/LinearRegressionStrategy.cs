using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : OptimisableStrategy
    {
        private int _window;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(512);

        public LinearRegressionStrategy(int window, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _window = window;
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            return Enumerable.Range(30, 200).Select(x =>
                new LinearRegressionStrategy(x, false));
        }

        public override void SetParameters(IStrategy strategy)
        {
            _window = ((LinearRegressionStrategy)strategy)._window;
        }

        public override bool ShouldAddFunds()
        {
            return true;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var latestPoints = MarketDataCache.Instance.GetLastSince(LatestDate, _window)
                .Select((x, i) => new XYPoint { X = i, Y = x.Price }).ToArray();
            if (latestPoints.Length < 2)
                return false;

            GenerateLinearBestFit(latestPoints, out double m, out double b);
            var prediction = (decimal) (m * MarketDataCache.Instance.Count - b);
            return data.Price < prediction;
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

            m = front / mid / back;
            b = m * meanX - ((double)meanY);
        }

        private class XYPoint
        {
            public int X;
            public decimal Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LinearRegressionStrategy strategy))
                return false;

            return strategy._window == _window;
        }

        public override int GetHashCode()
        {
            return _window.GetHashCode();
        }
    }
}
