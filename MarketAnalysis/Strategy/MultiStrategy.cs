using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class MultiStrategy : IStrategy
    {
        private readonly IStrategy[] _strategies;
        private readonly IStrategy _combinationRule;
        private static TimeSpan OptimisePeriod = TimeSpan.FromDays(524);
        private DateTime _latestDate;
        private DateTime? _lastOptimised;

        public object Key => new { _combinationRule?.Key };

        public MultiStrategy(IStrategy[] strategies, bool shouldOptimise = true)
        {
            _strategies = strategies;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?)null;
        }

        public bool ShouldOptimise()
        {
            if (_lastOptimised != null &&
                _latestDate > (_lastOptimised + OptimisePeriod))
            {
                _lastOptimised = _latestDate;
                return true;
            }
            return false;
        }

        public IEnumerable<IStrategy> Optimise()
        {
            return new IStrategy[0];

            //todo: fix
            //using (var progress = ProgressBarReporter.SpawnChild(_strategies.Count(), "Optimising..."))
            //{
            //    var strategyRules = new List<IStrategy>();
            //    var orderedStrategies = OrderStratergies(simulator, _strategies);
            //    while (orderedStrategies.Any())
            //    {
            //        var first = orderedStrategies.FirstOrDefault();
            //        var parentValue = simulator.Evaluate(first, _latestDate).LastOrDefault()?.Worth ?? 0m;

            //        var combination = GetCombinedStrategy(simulator, orderedStrategies);
            //        if (combination != null && combination.Value > parentValue)
            //        {
            //            strategyRules.Add(combination.AndStrategy);
            //            foreach (var s in combination.Strategies)
            //            {
            //                orderedStrategies.Remove(s);
            //            }
            //        }
            //        else
            //        {
            //            strategyRules.Add(first);
            //            orderedStrategies.Remove(first);
            //        }
            //        progress.Tick();
            //    }
            //    _combinationRule = new OrStrategy(strategyRules.ToArray());
            //}
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (_combinationRule == null)
                return false;

            if (data.Date > _latestDate)
                _latestDate = data.Date;

            return _combinationRule.ShouldBuyShares(data);
        }

        private List<IStrategy> OrderStratergies(ISimulator simulator, IStrategy[] strats)
        {
            return strats.Select(strategy =>
            {
                var result = simulator.Evaluate(strategy, _latestDate).LastOrDefault();
                return new { strategy, result, result?.BuyCount };
            }).OrderBy(x => x.BuyCount)
            .Select(x => x.strategy).ToList();
        }

        private CombinationResult GetCombinedStrategy(ISimulator simulator, List<IStrategy> orderedStrategies)
        {
            var combination = orderedStrategies.Select((s, i) =>
            {
                var collection = orderedStrategies.GetRange(0, i).ToArray();
                var andStrat = new AndStrategy(collection);
                var value = simulator.Evaluate(andStrat, _latestDate).LastOrDefault()?.Worth ?? 0m;
                return new CombinationResult
                {
                    Strategies = collection,
                    AndStrategy = andStrat,
                    Value = value
                };
            }).OrderByDescending(x => x.Value).FirstOrDefault();
            return combination.Strategies.Any() ? combination : null;
        }

        private class CombinationResult
        {
            public IStrategy[] Strategies { get; set; }
            public AndStrategy AndStrategy { get; set; }
            public decimal Value { get; set; }
        }

        private class AndStrategy : IStrategy
        {
            private IStrategy[] _strats;
            public object Key => _strats.Select(x => x.Key);

            public AndStrategy(IStrategy[] strats)
            {
                _strats = strats;
            }

            public bool ShouldOptimise()
            {
                return false;
            }

            public IEnumerable<IStrategy> Optimise()
            {
                return new IStrategy[0];
            }

            public bool ShouldAddFunds()
            {
                return true;
            }

            public bool ShouldBuyShares(MarketData data)
            {
                return _strats.All(x => x.ShouldBuyShares(data));
            }

            public override bool Equals(object obj)
            {
                return Equals(Key, (obj as AndStrategy)?.Key);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        private class OrStrategy : IStrategy
        {
            private IStrategy[] _strats;
            public object Key => _strats.Select(x => x.Key);

            public OrStrategy(IStrategy[] strats)
            {
                _strats = strats;
            }

            public bool ShouldOptimise()
            {
                return false;
            }

            public IEnumerable<IStrategy> Optimise()
            {
                return new IStrategy[0];
            }

            public bool ShouldAddFunds()
            {
                return true;
            }

            public bool ShouldBuyShares(MarketData data)
            {
                return _strats.Any(x => x.ShouldBuyShares(data));
            }

            public override bool Equals(object obj)
            {
                return Equals(Key, (obj as OrStrategy)?.Key);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(Key, (obj as MultiStrategy)?.Key);
        }

        public override int GetHashCode()
        {
            return Key?.GetHashCode() ?? 0;
        }
    }
}
