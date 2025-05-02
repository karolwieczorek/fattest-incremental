using System;
using System.Collections.Generic;

namespace Hypnagogia.Utils {
    public static class ListExtensions {
        public static T LastOrDefaultEfficient<T>(this IList<T> list, Func<T, bool> predicate) {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = list.Count - 1; i >= 0; i--) {
                if (predicate(list[i]))
                    return list[i];
            }

            return default;
        }
    }
}