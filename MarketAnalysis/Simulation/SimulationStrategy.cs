using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Strategy;
using System.Linq;

namespace MarketAnalysis.Simulation
{
    public abstract class StimulationStrategy
    {
        private SimulationCache _cache;

        public StimulationStrategy(SimulationCache cache)
        {
            _cache = cache;
        }

        public abstract SimulationState SimulateDay(IStrategy strategy, MarketData data);

        protected SimulationState GetPreviousState(IStrategy strategy, MarketData data)
        {
            var previousState = _cache.GetHistory(strategy)
                .Where(x => x.Date < data.Date).LastOrDefault() ?? new SimulationState();

            return new SimulationState
            {
                Date = previousState.Date,
                Funds = previousState.Funds,
                Shares = previousState.Shares,
                ShouldBuy = previousState.ShouldBuy,
                SharePrice = previousState.SharePrice,
                BuyCount = previousState.BuyCount
            };
        }

        protected SimulationState UpdateState(IStrategy strategy, MarketData data, SimulationState state)
        {
            state.Date = data.Date;
            state.SharePrice = data.Price;
            state.ShouldBuy = strategy.ShouldBuyShares(data);
            return state;
        }
    }
}
