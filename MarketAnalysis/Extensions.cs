using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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

        public static string GetDescription<T>(this T source)
            where T : Enum
        {
            FieldInfo field = typeof(T).GetField(source.ToString());
            return field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .Select(x => x.Description)
                        .FirstOrDefault();
        }

        public static (List<T> TrueSet, List<T> FalseSet) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var trueSet = new List<T>();
            var falseSet = new List<T>();

            foreach (var s in source)
            {
                if (predicate(s))
                    trueSet.Add(s);
                else
                    falseSet.Add(s);
            }
            return (trueSet, falseSet);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static decimal Clamp(this decimal value, int min, int max)
        {
            value = Math.Max(min, value);
            value = Math.Min(max, value);
            return value;
        }
    }
}
