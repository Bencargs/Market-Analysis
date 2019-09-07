using MarketAnalysis.Models;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class MultiStrategy : IStrategy
    {
        private IStrategy[] _strategies;
        private IStrategy _combinationRule;
        private List<Row> _history = new List<Row>(5000);

        public MultiStrategy(IStrategy[] strategies)
        {
            _strategies = strategies;
        }

        public void Optimise()
        {
            var simulator = new Simulation(_history, false);
            var strategyRules = new List<IStrategy>();
            var orderedStrategies = OrderStratergies(simulator, _strategies);
            while (orderedStrategies.Any())
            {
                var first = orderedStrategies.First();
                var parentValue = simulator.Evaluate(first).Worth;

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
            }
            _combinationRule = new OrStrategy(strategyRules.ToArray());
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            _history.Add(data);
            if (_combinationRule == null)
                return false;

            return _combinationRule.ShouldBuyShares(data);
        }

        private List<IStrategy> OrderStratergies(ISimulation simulator, IStrategy[] strats)
        {
            return strats.Select(strategy =>
            {
                var worth = simulator.Evaluate(strategy);
                return new { strategy, worth, simulator.BuyCount };
            }).OrderByDescending(x => x.BuyCount)
            .Select(x => x.strategy).ToList();
        }

        private CombinationResult GetCombinedStrategy(ISimulation simulator, List<IStrategy> orderedStrategies)
        {
            var combination = orderedStrategies.Select((s, i) =>
            {
                var collection = orderedStrategies.GetRange(0, i).ToArray();
                var andStrat = new AndStrategy(collection);
                var value = simulator.Evaluate(andStrat).Worth;
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

            public AndStrategy(IStrategy[] strats)
            {
                _strats = strats;
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
        }

        private class OrStrategy : IStrategy
        {
            private IStrategy[] _strats;

            public OrStrategy(IStrategy[] strats)
            {
                _strats = strats;
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
        }
    }
}
