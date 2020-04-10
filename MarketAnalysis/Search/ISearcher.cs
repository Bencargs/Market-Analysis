using MarketAnalysis.Strategy;
using System;

namespace MarketAnalysis.Search
{
	public interface ISearcher
	{
		IStrategy Maximum(DateTime endDate);
	}
}
