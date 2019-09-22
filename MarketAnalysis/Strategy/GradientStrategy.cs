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
        private List<Row> _history = new List<Row>(5000);

        public GradientStrategy(int window, decimal threshold)
        {
            _window = window;
            _threshold = threshold;
        }

        public void Optimise()
        {
            return;

            var simulator = new Simulation(_history, false);
            var optimal = Enumerable.Range(1, 10).SelectMany(x =>
            {
                return Enumerable.Range(20, 20).Select(window =>
                {
                    var threshold = -((decimal)x / 100);
                    var result = simulator.Evaluate(new GradientStrategy(window, threshold));
                    return new { threshold, window, result.Worth, simulator.BuyCount };
                });
            }).OrderByDescending(x => x.Worth).ThenByDescending(x => x.BuyCount).ToArray();
            _window = optimal.First().window;
            _threshold = optimal.First().threshold;
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
    }
}
