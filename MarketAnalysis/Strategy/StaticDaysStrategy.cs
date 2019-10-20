using MarketAnalysis.Models;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class StaticDatesStrategy : IStrategy
    {
        private bool[] _buyDates;
        private int _i;

        public object Key => _buyDates;

        public StaticDatesStrategy(bool[] buyDates)
        {
            _buyDates = buyDates;
        }

        public bool ShouldOptimise()
        {
            return false;
        }

        public void Optimise()
        {
            return;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            return (_buyDates[_i++]);
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
