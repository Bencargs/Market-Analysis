//using MarketAnalysis.Models;
//using MarketAnalysis.Simulation;
//using ShellProgressBar;
//using System;

//namespace MarketAnalysis.Strategy
//{
//    public abstract class OptimisableStrategy : IStrategy
//    {
//        private DateTime? _lastOptimised;
//        public abstract StrategyType StrategyType { get; }
//        protected abstract TimeSpan OptimisePeriod { get; }
//        public DateTime LatestDate { get; protected set; }

//        protected OptimisableStrategy(bool shouldOptimise)
//        {
//            _lastOptimised = shouldOptimise 
//                ? DateTime.MinValue 
//                : (DateTime?)null;
//        }

//        public void Optimise(ISimulator simulator, IProgressBar progress)
//        {
//            if (!ShouldOptimise())
//                return;

//            var optimal = GetOptimum(simulator, progress);

//            SetParameters(optimal);
//        }

//        protected abstract IStrategy GetOptimum(ISimulator simulator, IProgressBar progress);

//        public bool ShouldBuyShares(MarketData data)
//        {
//            if (data.Date > LatestDate)
//                LatestDate = data.Date;

//            return ShouldBuy(data);
//        }

//        private bool ShouldOptimise()
//        {
//            if (_lastOptimised != null &&
//                LatestDate > (_lastOptimised + OptimisePeriod))
//            {
//                _lastOptimised = LatestDate;
//                return true;
//            }
//            return false;
//        }

//        protected abstract void SetParameters(IStrategy strategy);

//        protected abstract bool ShouldBuy(MarketData data);
//    }
//}
