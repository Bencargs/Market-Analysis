using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Search
{
	public interface ISearcher
	{
		T Maximum<T>(IEnumerable<T> strategies, DateTime endDate)
			where T : IStrategy;
	}
}
