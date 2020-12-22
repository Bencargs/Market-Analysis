﻿using MarketAnalysis.Strategy;
using MarketAnalysis.Strategy.Parameters;
using System;
using System.Collections.Generic;

namespace MarketAnalysis.Search
{
	public interface ISearcher
	{
		public IStrategy Maximum(
			IEnumerable<IParameters> parameters,
			DateTime fromDate,
			DateTime endDate);
	}
}
