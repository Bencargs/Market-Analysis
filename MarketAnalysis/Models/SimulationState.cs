using MarketAnalysis.Simulation;
using System;

namespace MarketAnalysis.Models
{
    public class SimulationState : IEquatable<SimulationState>
    {
        public DateTime Date { get; set; }
        public decimal TotalFunds { get; set; }
        public decimal Shares { get; set; }
        public decimal OrderValue { get; set; }
        public OrderQueue OrderQueue { get; set; } = new();
        public bool ShouldBuy { get; set; }
        public decimal SharePrice { get; set; }
        public int BuyCount { get; set; }
        public decimal Worth => TotalFunds + OrderQueue.Worth() + (Shares * SharePrice);

        public SimulationState UpdateState(
            MarketData data,
            bool shouldBuy)
        {
            return new()
            {
                Date = data.Date,
                SharePrice = data.Price,
                ShouldBuy = shouldBuy,
                TotalFunds = TotalFunds,
                Shares = Shares,
                OrderQueue = OrderQueue,
                OrderValue = OrderQueue.Worth(),
                BuyCount = BuyCount,
            };
        }

        public void AddFunds(decimal dailyFunds)
            => TotalFunds += dailyFunds;

        public void AddBuyOrder(decimal brokerage, int orderDelay, decimal funds)
        {
            var cost = funds - brokerage;
            if ((cost - SharePrice) <= 0)
                return;

            TotalFunds -= cost;
            var order = new MarketOrder
            {
                Funds = cost,
                ExecutionDate = Date.AddDays(orderDelay),
            };
            OrderQueue.Add(order);
        }

        public void ExecuteOrders()
        {
            foreach (var order in OrderQueue.Get(Date))
            {
                var newShares = order.Funds / SharePrice;
                Shares += newShares;
                BuyCount++;
            }
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
                TotalFunds,
                Shares,
                OrderQueue.Worth(),
                ShouldBuy,
                BuyCount);
        }

        public bool Equals(SimulationState other)
        {
            return Date == other.Date &&
                   TotalFunds == other.TotalFunds &&
                   Shares == other.Shares &&
                   OrderQueue.Worth() == other.OrderQueue.Worth() &&
                   ShouldBuy == other.ShouldBuy &&
                   BuyCount == other.BuyCount;
        }
    }
}
