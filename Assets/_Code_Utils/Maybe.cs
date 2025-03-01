using System;
using Newtonsoft.Json;

namespace Hypnagogia.Utils {
    public struct Maybe<T> : IMaybe<T> {
        [JsonProperty]
        readonly T value;
        public readonly bool hasValue;

        public bool HasValue => hasValue;

        [JsonConstructor]
        Maybe(T value, bool hasValue) {
            this.value = value;
            this.hasValue = hasValue;
        }

        public static Maybe<T> Empty => new(default, false);
        public static Maybe<T> IfNotNull(T value) => value == null ? Empty : WithValue(value);
        public static Maybe<T> WithValue(T value) => new(value, true);

        public bool TryToGetValue(out T value) {
            value = this.value;
            return hasValue;
        }

        public T ValueOr(T other) {
            return hasValue ? value : other;
        }

        public bool ValueEquals(T other) {
            return hasValue && value.Equals(other);
        }

        public void ForValue(Action<T> handler) {
            if (TryToGetValue(out var myValue))
                handler(myValue);
        }

        public static implicit operator Maybe<T>(T value) => WithValue(value);

        public override string ToString() {
            return $"Maybe<{typeof(T).Name}>:" + (hasValue ? $"{value.ToString()}" : "Empty");
        }
    }
}