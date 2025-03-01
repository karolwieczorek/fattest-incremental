using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypnagogia.Utils {
    public static class EnumerableExtensions {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null)
                return null;

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (T obj in source)
                action(obj);

            // ReSharper disable once PossibleMultipleEnumeration
            return source;
        }

        public static T GetRandomElement<T>(this IEnumerable<T> source) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source as IList<T> ?? source.ToList();
            if (list.Count == 0)
                throw new InvalidOperationException("Sequence contains no elements");

            var rand = new Random();
            var index = rand.Next(list.Count);
            return list[index];
        }
    }
}