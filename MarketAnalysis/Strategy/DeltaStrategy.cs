using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy
    {
        private int _threshold;
        private List<Row> _history = new List<Row>(5000);

        public DeltaStrategy(int threshold)
        {
            _threshold = threshold;
        }

        public void Optimise()
        {
            var simulator = new Simulation(_history, false);
            var optimal = Enumerable.Range(1, 200).Select(x =>
            {
                var worth = simulator.Evaluate(new LinearRegressionStrategy(x));
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

            return data.Delta < _threshold;
        }
    }
}
