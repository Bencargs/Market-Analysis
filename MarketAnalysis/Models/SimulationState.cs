using MarketAnalysis.Simulation;
using System;

namespace MarketAnalysis.Models
{
    public class SimulationState
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
            return new SimulationState
            {
                Date = data.Date,
                SharePrice = data.Price,
                ShouldBuy = shouldBuy,
                Funds = Funds,
                Shares = Shares,
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
        }

        public void ExecuteOrders(OrderQueue orderQueue)
        {
            foreach (var order in orderQueue.Get(Date))
            {
                var newShares = order.Funds / SharePrice;
                Shares += newShares;
                BuyCount++;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SimulationState other))
                return false;

            return Date == other.Date &&
                   Funds == other.Funds &&
                   Shares == other.Shares &&
                   ShouldBuy == other.ShouldBuy &&
                   BuyCount == other.BuyCount;
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode() ^
                   Funds.GetHashCode() ^
                   Shares.GetHashCode() ^
                   ShouldBuy.GetHashCode() ^
                   BuyCount.GetHashCode();
        }
    }
}
