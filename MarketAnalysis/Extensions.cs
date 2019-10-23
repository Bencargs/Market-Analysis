using System;
using System.Collections.Generic;

namespace MarketAnalysis
{
    public static class Extensions
    {
        public static int IndexOfMin(this IList<decimal> self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.Count == 0)
            {
                throw new ArgumentException("List is empty.", "self");
            }

            var min = self[0];
            int minIndex = 0;

            for (int i = 1; i < self.Count; ++i)
            {
                if (self[i] < min)
                {
                    min = self[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        public static T[] Last<T>(this List<T> source, int elements)
        {
            var start = Math.Max(0, source.Count - elements);
            var count = Math.Min(source.Count, elements);
            var batch = source.GetRange(start, count).ToArray();

            return batch;
        }
    }
}
