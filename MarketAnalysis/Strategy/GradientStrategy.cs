using System;
using System.Collections.Generic;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MathNet.Numerics;

namespace MarketAnalysis.Strategy
{
    public class GradientStrategy : IStrategy
    {
        private readonly int _window;
        private readonly decimal _threshold;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 1024;
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
                   count % OptimisePeriod == 0;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return Enumerable.Range(1, 10).SelectMany(x =>
            {
                return Enumerable.Range(20, 20).Select(window =>
                {
                    var threshold = -((decimal)x / 100);
                    return new GradientStrategy(window, threshold, false);
                });
            });
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
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
