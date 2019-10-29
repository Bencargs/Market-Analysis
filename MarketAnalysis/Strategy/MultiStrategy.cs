using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class MultiStrategy : IStrategy
    {
        private IStrategy[] _strategies;
        private IStrategy _combinationRule;
        private readonly bool _shouldOptimise;
        private const int OptimisePeriod = 524;
        private List<Row> _history = new List<Row>(5000);

        public object Key => new { _combinationRule?.Key };

        public MultiStrategy(IStrategy[] strategies, bool shouldOptimise = true)
        {
            _strategies = strategies;
            _shouldOptimise = shouldOptimise;
        }

        public bool ShouldOptimise()
        {
            return _shouldOptimise &&
                   _history.Count % OptimisePeriod == 0;
        }

        public void Optimise()
        {
            using (var progress = ProgressBarReporter.SpawnChild(_strategies.Count(), "Optimising..."))
            {
                var simulator = new Simulator(_history);
                var strategyRules = new List<IStrategy>();
                var orderedStrategies = OrderStratergies(simulator, _strategies);
                while (orderedStrategies.Any())
                {
                    var first = orderedStrategies.FirstOrDefault();
                    var parentValue = simulator.Evaluate(first).LastOrDefault()?.Worth ?? 0m;

                    var combination = GetCombinedStrategy(simulator, orderedStrategies);
                    if (combination != null && combination.Value > parentValue)
                    {
                        strategyRules.Add(combination.AndStrategy);
                        foreach (var s in combination.Strategies)
                        {
                            orderedStrategies.Remove(s);
                        }
                    }
                    else
                    {
                        strategyRules.Add(first);
                        orderedStrategies.Remove(first);
                    }
                    progress.Tick();
                }
                _combinationRule = new OrStrategy(strategyRules.ToArray());
            }
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            if (_combinationRule == null)
                return false;

            if (!_history.Any(x => x.Date == data.Date))
                _history.Add(data);

            return _combinationRule.ShouldBuyShares(data);
        }

        private List<IStrategy> OrderStratergies(ISimulator simulator, IStrategy[] strats)
        {
            return strats.Select(strategy =>
            {
                var result = simulator.Evaluate(strategy).LastOrDefault();
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
                var value = simulator.Evaluate(andStrat).LastOrDefault()?.Worth ?? 0m;
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
