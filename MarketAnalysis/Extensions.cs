﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            T[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;

                if (count != size)
                    continue;

                yield return bucket.Select(x => x);

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                Array.Resize(ref bucket, count);
                yield return bucket.Select(x => x);
            }
        }
    }
}
