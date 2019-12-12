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
        private int _window;
        private decimal _threshold;
        private DateTime _latestDate;
        private DateTime? _lastOptimised;
        private static readonly TimeSpan OptimisePeriod = TimeSpan.FromDays(512);

        public GradientStrategy(int window, decimal threshold, bool shouldOptimise = true)
        {
            _window = window;
            _threshold = threshold;
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

        public IEnumerable<IStrategy> GetOptimisations()
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

        public void SetParameters(IStrategy strategy)
        {
            var optimal = (GradientStrategy)strategy;
            _window = optimal._window;
            _threshold = optimal._threshold;
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
            if (!(obj is GradientStrategy strategy))
                return false;

            return strategy._window == _window &&
                   strategy._threshold == _threshold;
        }

        public override int GetHashCode()
        {
            return _window.GetHashCode() ^ _threshold.GetHashCode();
        }
    }
}
