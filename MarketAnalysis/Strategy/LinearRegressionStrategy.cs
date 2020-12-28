using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class LinearRegressionStrategy : IStrategy, IEquatable<LinearRegressionStrategy>
    {
        private readonly ISearcher _searcher;
        private readonly IMarketDataCache _marketDataCache;
        private LinearRegressionParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.LinearRegression;

        public LinearRegressionStrategy(
            IMarketDataCache marketDataCache,
            ISearcher searcher,
            LinearRegressionParameters parameters)
        {
            _searcher = searcher;
            _marketDataCache = marketDataCache;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(30, 200).Select(x => 
                new LinearRegressionParameters{ Lookback = x });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (LinearRegressionParameters)optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            var latestPoints = _marketDataCache
                .GetLastSince(data.Date, _parameters.Lookback)
                .Select((x, i) => new XYPoint { X = i, Y = x.Price })
                .ToArray();

            if (latestPoints.Length < 2)
                return false;

            GenerateLinearBestFit(latestPoints, out double m, out double b);
            var prediction = (decimal) (m * _marketDataCache.Count - b);
            return data.Price < prediction;
        }

        private static void GenerateLinearBestFit(XYPoint[] points, out double m, out double b)
        {
            int numPoints = points.Length;
            double meanX = points.Average(point => point.X);
            decimal meanY = points.Average(point => point.Y);

            double sumXSquared = Math.Pow(points.Sum(x => x.X), 2);
            decimal sumXY = points.Sum(point => point.X * point.Y);

            var meanSqrd = Math.Pow(meanX, 2);
            var back = sumXSquared / numPoints - meanSqrd;
            var mid = numPoints - meanX * ((double)meanY);
            var front = (double)sumXY;

            m = front / mid / back;
            b = m * meanX - ((double)meanY);
        }

        private struct XYPoint
        {
            public int X;
            public decimal Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LinearRegressionStrategy)) return false;

            return Equals(obj as LinearRegressionStrategy);
        }

        public bool Equals(LinearRegressionStrategy strategy)
        {
            return strategy._parameters.Lookback == _parameters.Lookback;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Lookback);
        }
    }
}
