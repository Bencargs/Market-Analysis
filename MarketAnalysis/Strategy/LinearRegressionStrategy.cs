using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : IStrategy
    {
        private int _window;
        private List<Row> _history = new List<Row>(5000);

        public LinearRegressionStrategy(int window)
        {
            _window = window;
        }

        public void Optimise()
        {
            return;

            var simulator = new Simulation(_history, false);
            var optimal = Enumerable.Range(30, 200).Select(x =>
            {
                var result = simulator.Evaluate(new LinearRegressionStrategy(x));
                return new { x, result.Worth, simulator.BuyCount };
            }).OrderByDescending(x => x.Worth).ThenBy(x => x.BuyCount).First();
            _window = optimal.x;
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
    }
}
