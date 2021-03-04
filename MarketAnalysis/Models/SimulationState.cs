using MarketAnalysis.Simulation;
using System;

namespace MarketAnalysis.Models
{
    public class SimulationState : IEquatable<SimulationState>
    {
        public DateTime Date { get; set; }
        public decimal Funds { get; set; }
        public decimal Shares { get; set; }
        public decimal Orders { get; set; }
        public bool ShouldBuy { get; set; }
        public decimal SharePrice { get; set; }
        public int BuyCount { get; set; }
        public decimal Worth => Funds + Orders + (Shares * SharePrice);

        public SimulationState UpdateState(
            MarketData data,
            bool shouldBuy)
        {
            return new()
            {
                Date = data.Date,
                SharePrice = data.Price,
                ShouldBuy = shouldBuy,
                Funds = Funds,
                Shares = Shares,
                Orders = Orders,
                BuyCount = BuyCount,
            };
        }

        public void AddFunds(Investor investor)
            => Funds += investor.DailyFunds;

        public void AddBuyOrder(Investor investor, OrderQueue orderQueue)
        {
            var cost = Funds - investor.OrderBrokerage;
            if (cost <= 0)
                return;

            Funds = 0;
            var order = new MarketOrder
            {
                Funds = cost,
                ExecutionDate = Date.AddDays(investor.OrderDelayDays),
            };
            orderQueue.Add(order);
            Orders = orderQueue.Worth();
        }

        public void ExecuteOrders(OrderQueue orderQueue)
        {
            foreach (var order in orderQueue.Get(Date))
            {
                var newShares = order.Funds / SharePrice;
                Shares += newShares;
                BuyCount++;
            }
            Orders = orderQueue.Worth();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SimulationState)) return false;

            return Equals(obj as SimulationState);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Date,
                Funds,
                Shares,
                Orders,
                ShouldBuy,
                BuyCount);
        }

        public bool Equals(SimulationState other)
        {
            return Date == other.Date &&
                   Funds == other.Funds &&
                   Shares == other.Shares &&
                   Orders == other.Orders &&
                   ShouldBuy == other.ShouldBuy &&
                   BuyCount == other.BuyCount;
        }
    }
}
