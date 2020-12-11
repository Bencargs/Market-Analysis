//using MarketAnalysis.Models;
//using MarketAnalysis.Providers;
//using MarketAnalysis.Strategy;
//using ShellProgressBar;

//namespace MarketAnalysis.Simulation
//{
//    public class BacktestingSimulator : IStimulationStrategy
//    {
//        private readonly ISimulator _simulator;
//        private readonly InvestorProvider _investorProvider;
//        private readonly ProgressBarProvider _progressProvider;

//        public BacktestingSimulator(
//            ISimulator simulator, 
//            InvestorProvider investorProvider, 
//            ProgressBarProvider progressProvider)
//        {
//            _simulator = simulator;
//            _investorProvider = investorProvider;
//            _progressProvider = progressProvider;
//        }

//        public SimulationState SimulateDay(
//            IStrategy strategy,
//            MarketData data, 
//            OrderQueue queue, 
//            SimulationState previousState, 
//            ChildProgressBar progress)
//        {
//            using var childProgress = _progressProvider.Create(progress, 0, "Optimising...");
//            var investor = _investorProvider.Current;
//            var state = UpdateState(strategy, data, previousState);

//            AddFunds(investor, state);
//            ExecuteOrders(state, queue);

//            if (strategy is OptimisableStrategy optimisable)
//                optimisable.Optimise(_simulator, childProgress);

//            if (state.ShouldBuy)
//                AddBuyOrder(investor, state, queue);

//            return state;
//        }

//        private SimulationState UpdateState(IStrategy strategy, MarketData data, SimulationState previousState)
//        {
//            var shouldBuy = strategy.ShouldBuyShares(data);

//            return new SimulationState
//            {
//                Date = data.Date,
//                SharePrice = data.Price,
//                ShouldBuy = shouldBuy,
//                Funds = previousState.Funds,
//                Shares = previousState.Shares,
//                BuyCount = previousState.BuyCount,
//            };
//        }

//        private void AddBuyOrder(Investor investor, SimulationState state, OrderQueue orderQueue)
//        {
//            var cost = state.Funds - investor.OrderBrokerage;
//            if (cost <= 0)
//                return;

//            state.Funds  = 0;
//            var order = new MarketOrder
//            {
//                Funds = cost,
//                ExecutionDate = state.Date.AddDays(investor.OrderDelayDays),
//            };
//            orderQueue.Add(order);
//        }

//        private void ExecuteOrders(SimulationState state, OrderQueue orderQueue)
//        {
//            foreach (var order in orderQueue.Get(state.Date))
//            {
//                var newShares = order.Funds / state.SharePrice;
//                state.Shares += newShares;
//                state.BuyCount++;
//            }
//        }

//        private void AddFunds(Investor investor, SimulationState state)
//        {
//            state.Funds += investor.DailyFunds;
//        }
//    }
//}
