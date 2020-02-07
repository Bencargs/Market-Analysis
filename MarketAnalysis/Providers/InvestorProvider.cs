using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class InvestorProvider : IEnumerator<Investor>
    {
        private readonly IRepository<Investor> _investorRepository;
        private IEnumerator<Investor> _investors;
        public Investor Current { get; private set; }

        object IEnumerator.Current => Current;

        public InvestorProvider(IRepository<Investor> investorRepository)
        {
            _investorRepository = investorRepository;
        }

        public async Task Initialise()
        {
            var accessor = await _investorRepository.Get();
            _investors = accessor.GetEnumerator();
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
