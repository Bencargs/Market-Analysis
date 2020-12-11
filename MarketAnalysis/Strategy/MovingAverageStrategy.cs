//using MarketAnalysis.Caching;
//using MarketAnalysis.Models;
//using MarketAnalysis.Search;
//using MarketAnalysis.Simulation;
//using ShellProgressBar;
//using System;
//using System.Linq;

//namespace MarketAnalysis.Strategy
//{
//    public class MovingAverageStrategy : OptimisableStrategy
//    {
//        private int _window;
//        private double _threshold;
//        private readonly MarketDataCache _marketDataCache;
//        public override StrategyType StrategyType { get; } = StrategyType.MovingAverage;
//        protected override TimeSpan OptimisePeriod { get; } = TimeSpan.FromDays(1024);

//        public MovingAverageStrategy(MarketDataCache marketDataCache)
//            : this (marketDataCache, 0, 0)
//        { }

//        public MovingAverageStrategy(
//            MarketDataCache marketDataCache,
//            int window, 
//            double threshold, 
//            bool shouldOptimise = true)
//            : base(shouldOptimise)
//        {
//            _window = window;
//            _threshold = threshold;
//            _marketDataCache = marketDataCache;
//        }

//        protected override IStrategy GetOptimum(ISimulator simulator, IProgressBar progress)
//        {
//            var potentials = Enumerable.Range(1, 90).SelectMany(w =>
//            {
//                return Enumerable.Range(1, 60).Select(t =>
//                {
//                    var threshold = (double)t / 10;
//                    return new MovingAverageStrategy(_marketDataCache, w, threshold, false);
//                });
//            });

//            var searcher = new LinearSearch(simulator, potentials, progress);
//            return searcher.Maximum(LatestDate);
//        }

//        protected override void SetParameters(IStrategy strategy)
//        {
//            var optimal = ((MovingAverageStrategy)strategy);
//            _window = optimal._window;
//            _threshold = optimal._threshold;
//        }

//        protected override bool ShouldBuy(MarketData data)
//        {
//            var batch = _marketDataCache.GetLastSince(LatestDate, _window).Select(x => x.Price).ToArray();
//            if (batch.Length < 2)
//                return false;

//            var mean = batch.Average();
//            double sum = batch.Sum(d => Math.Pow((double)(d - mean), 2));
//            var a = Math.Abs( sum / batch.Count() - 1 );
//            var standardDeviation = Math.Sqrt(a);
//            var weightedDeviation = (decimal) (standardDeviation * _threshold);
            
//            if (data.Price < (mean - weightedDeviation))
//                return true;
//            return false;
//        }

//        public override bool Equals(object obj)
//        {
//            if (!(obj is MovingAverageStrategy strategy))
//                return false;

//            return strategy._window == _window
//                && strategy._threshold == _threshold;
//        }

//        public override int GetHashCode()
//        {
//            return _window.GetHashCode() ^ 
//                _threshold.GetHashCode() ^ 
//                397;
//        }
//    }
//}
