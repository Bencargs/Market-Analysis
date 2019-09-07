using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class RelativeStrengthStrategy : IStrategy
    {
        private int _threshold;
        private List<Row> _history = new List<Row>(5000);

        public RelativeStrengthStrategy(int threshold)
        {
            _threshold = threshold;
        }

        public void Optimise()
        {
            var simulator = new Simulation(_history, false);
            var optimal = Enumerable.Range(0, 100).Select(x =>
            {
                var worth = simulator.Evaluate(new RelativeStrengthStrategy(x));
                return new { x, worth, simulator.BuyCount };
            }).OrderByDescending(x => x.worth).ThenBy(x => x.BuyCount).First();
            _threshold = optimal.x;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            if (!_history.Any(x => x.Date == data.Date))
                _history.Add(data);
            if (_history.Count < 3)
                return false;

            var batch = _history.AsEnumerable().Reverse().Take(_threshold).Reverse().ToList();
            var strength = GetRelativeStrength(data.Price, batch);

            return new[] { 0, 3, 5, 6 }.Contains(strength);
        }

        private int GetRelativeStrength(decimal price, List<Row> data)
        {
            var min = data.Min(y => y.Price);
            var max = data.Max(y => y.Price);
            var range = max - min;
            if (range == 0)
                return 100;

            var adjustedPrice = price - min;
            return Convert.ToInt32(adjustedPrice / range * 100);
        }
    }
}
