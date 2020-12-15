using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Search;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class VolumeStrategy : IStrategy
    {
        private readonly ISearcher _searcher;
        private readonly StrategyFactory _strategyFactory;
        private VolumeParameters _parameters;

        public IParameters Parameters 
        {
            get => _parameters;
            private set => _parameters = (VolumeParameters)value; 
        }
        public StrategyType StrategyType { get; } = StrategyType.Volume;

        public VolumeStrategy(
            StrategyFactory strategyFactory,
            ISearcher searcher,
            VolumeParameters parameters)
        {
            _strategyFactory = strategyFactory;
            _searcher = searcher;

            Parameters = parameters;
        }

        public void Optimise(DateTime fromDate, DateTime endDate)
        {
            var potentials = Enumerable.Range(1, 800).Select(x =>
                _strategyFactory.Create(new VolumeParameters { Threshold = x }));

            var optimum = _searcher.Maximum(potentials, fromDate, endDate);

            Parameters = ((VolumeStrategy)optimum).Parameters;
        }

        public bool ShouldBuy(MarketData data)
        {
            var shouldBuy = _parameters.PreviousVolume != 0 &&
                (data.Volume / _parameters.PreviousVolume) > _parameters.Threshold;

            _parameters.PreviousVolume = data.Volume;
            return shouldBuy;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is VolumeStrategy strategy))
                return false;

            return _parameters.Threshold == strategy._parameters.Threshold;
        }

        public override int GetHashCode()
        {
            return _parameters.Threshold.GetHashCode();
        }
    }
}
