using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class DeltaStrategy : IStrategy, IEquatable<DeltaStrategy>
    {
        private readonly ISearcher _searcher;
        private DeltaParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Delta;

        public DeltaStrategy(
            ISearcher searcher,
            DeltaParameters parameters)
        {
            _searcher = searcher;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(1, 100).Select(x =>
            {
                var threshold = (decimal)x / 1000;
                return new DeltaParameters { Threshold = threshold };
            });

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            _parameters = (DeltaParameters) optimum.Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            return data.Delta < _parameters.Threshold;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DeltaStrategy)) return false;

            return Equals(obj as DeltaStrategy);
        }

        public bool Equals(DeltaStrategy strategy)
        {
            return strategy._parameters.Threshold == _parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_parameters.Threshold);
        }
    }
}
