using System;
using System.Linq;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;

namespace MarketAnalysis.Strategy
{
    public class SpreadStrategy : IStrategy, IEquatable<SpreadStrategy>
    {
        private readonly ISearcher _searcher;
        private SpreadParameters _parameters;

        public IParameters Parameters => _parameters;
        public StrategyType StrategyType { get; } = StrategyType.Spread;

        public SpreadStrategy(
            ISearcher searcher,
            SpreadParameters parameters)
        {
            _searcher = searcher;
            _parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime toDate)
        {
            var potentials = Enumerable.Range(0, 100)
                .Select(x => new SpreadParameters {Threshold = (decimal)x/100});

            var optimum = _searcher.Maximum(potentials, fromDate, toDate);

            _parameters = (SpreadParameters) optimum;
        }

        public bool ShouldBuy(MarketData data)
            => data.SpreadPercent > _parameters.Threshold;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(SpreadStrategy)) return false;

            return Equals(obj as SpreadStrategy);
        }

        public bool Equals(SpreadStrategy other)
            => other._parameters.Threshold == _parameters.Threshold;

        public override int GetHashCode()
            => HashCode.Combine(_parameters.Threshold);
    }
}
