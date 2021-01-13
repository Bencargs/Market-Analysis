using System;
using System.Linq;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class OddsStrategy : IStrategy, IEquatable<OddsStrategy>
    {
        private const double WaitRatio = 1 / Math.E;
        private readonly ISearcher _searcher;
        private OddsParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Odds;
        
        public OddsStrategy(
            ISearcher searcher,
            OddsParameters parameters)
        {
            _searcher = searcher;
            _parameters = parameters;
        }


        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            var potentials = Enumerable.Range(3, 60)
                .Select(wait => new OddsParameters
                {
                    WaitTime = 0,
                    MaxWaitTime = wait,
                    MinPrice = decimal.MaxValue,
                });

            var optimum = _searcher.Maximum(potentials, fromDate, toDate);

            _parameters = (OddsParameters) optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            // Still during monitoring period - keep waiting
            var monitorPeriod = WaitRatio * _parameters.MaxWaitTime;
            if (++_parameters.WaitTime < monitorPeriod)
            {
                _parameters.MinPrice = Math.Min(data.Price, _parameters.MinPrice);
                return false;
            }

            // Past maximum wait period - action immediately
            if (_parameters.WaitTime > _parameters.MaxWaitTime)
            {
                _parameters.MinPrice = data.Price;
                _parameters.WaitTime = 0;
                return true;
            }

            // If price is better than previous min - action
            if (data.Price < _parameters.MinPrice)
            {
                _parameters.MinPrice = data.Price;
                return true;
            }

            return false;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(OddsStrategy)) return false;

            return Equals(obj as OddsStrategy);
        }

        public bool Equals(OddsStrategy other)
            => other._parameters.MinPrice == _parameters.MinPrice &&
               other._parameters.WaitTime == _parameters.WaitTime &&
               other._parameters.MaxWaitTime == _parameters.MaxWaitTime;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.MinPrice, _parameters.WaitTime, _parameters.MaxWaitTime);
    }
}
