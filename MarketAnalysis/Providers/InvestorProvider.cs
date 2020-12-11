using MarketAnalysis.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Providers
{
    public class InvestorProvider : IEnumerator<Investor>
    {
        private IEnumerator<Investor> _investors;
        public Investor Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Initialise()
        {
            _investors = new[]
            {
                    new Investor
                    {
                        Name = "Benjamin Cargill",
                        Number = "000001",
                        Email = "benjamin.d.cargill@gmail.com",
                        DailyFunds = 10m,
                        OrderBrokerage = 0m,
                        OrderDelayDays = 3
                    },
                    //new RecipientDetails
                    //{
                    //    Date = DateTime.Now,
                    //    Name = "Cyndi Chen",
                    //    Number = "000002",
                    //    Email = "annsn12@hotmail.com"
                    //}
            }.Cast<Investor>().GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_investors.MoveNext())
            {
                Current = _investors.Current;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _investors.Reset();
        }

        public IEnumerator<Investor> GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            _investors.Dispose();
        }
    }
}
