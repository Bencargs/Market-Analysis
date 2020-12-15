using MarketAnalysis.Strategy;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Search
{
	public interface ISearcher
	{
		T Maximum<T>(
			IEnumerable<T> strategies, 
			DateTime fromDate, 
			DateTime endDate)
			where T : IStrategy;
	}
}
