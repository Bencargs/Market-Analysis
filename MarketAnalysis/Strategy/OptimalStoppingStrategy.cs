using System;
using System.Linq;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class OptimalStoppingStrategy : IStrategy, IEquatable<OptimalStoppingStrategy>
    {
        private const double WaitRatio = 1 / Math.E; // Via Odds Algorithm
        private readonly ISearcher _searcher;
        private OptimalStoppingParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.OptimalStopping;
        
        public OptimalStoppingStrategy(
            ISearcher searcher,
            OptimalStoppingParameters parameters)
        {
            _searcher = searcher;
            _parameters = parameters;
        }


        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            var potentials = Enumerable.Range(3, 60)
                .Select(wait => new OptimalStoppingParameters
                {
                    WaitTime = 0,
                    MaxWaitTime = wait,
                    MinPrice = decimal.MaxValue,
                });

            var optimum = _searcher.Maximum(potentials, fromDate, toDate);

            _parameters = (OptimalStoppingParameters) optimum.Parameters;
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
            if (obj.GetType() != typeof(OptimalStoppingStrategy)) return false;

            return Equals(obj as OptimalStoppingStrategy);
        }

        public bool Equals(OptimalStoppingStrategy other)
            => other._parameters.MinPrice == _parameters.MinPrice &&
               other._parameters.WaitTime == _parameters.WaitTime &&
               other._parameters.MaxWaitTime == _parameters.MaxWaitTime;

        public override int GetHashCode()
            => HashCode.Combine(
                _parameters.MinPrice, 
                _parameters.WaitTime, 
                _parameters.MaxWaitTime);
    }
}
