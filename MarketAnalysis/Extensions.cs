using System;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public static class Extensions
    {
        public static T[] Last<T>(this List<T> source, int elements)
        {
            var start = Math.Max(0, source.Count - elements);
            var count = Math.Min(source.Count, elements);
            var batch = source.GetRange(start, count).ToArray();

            return batch;
        }
    }
}
