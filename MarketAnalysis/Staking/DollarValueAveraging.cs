using System;
using System.Linq;
using MarketAnalysis.Caching;
using MarketAnalysis.Models;

namespace MarketAnalysis.Staking
{
    public class DollarValueAveraging : IStakingService
    {
        private MarketData _previousPrice;
        private readonly IMarketDataCache _cache;

        public DollarValueAveraging(IMarketDataCache cache)
        {
            _cache = cache;
        }

        public void Evaluate(DateTime fromDate, DateTime toDate)
        { }

        public decimal GetStake(DateTime today, decimal totalFunds)
        {
            var latestDate = _cache.GetLastSince(today, 1).LastOrDefault();
            if (latestDate == null || latestDate.Price == 0)
                return 0;

            if (_previousPrice == null)
            {
                _previousPrice = latestDate;
                return totalFunds;
            }

            var delta = _previousPrice.Price / latestDate.Price;
            var fraction = delta.Clamp(0, 1);
            return totalFunds * fraction;
        }
    }
}
