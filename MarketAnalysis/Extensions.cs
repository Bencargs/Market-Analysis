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

        public static (IEnumerable<T>, IEnumerable<T>) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var list1 = new List<T>();
            var list2 = new List<T>();

            foreach (var s in source)
            {
                if (predicate(s))
                    list1.Add(s);
                else
                    list2.Add(s);
            }
            return (list1, list2);
        }
    }
}
