using System;

namespace Hypnagogia.Utils {
    public class ObservableMaybe<T> : IReadOnlyObservable<Maybe<T>>, IMaybe<T>{
        Maybe<T> value;
        bool isValueType;

        public event Action<Maybe<T>> ChangedValue;
        public event Action Changed;

        public ObservableMaybe(T value = default) {
            this.value = value;
            isValueType = typeof(T).IsValueType;
        }

        public bool HasValue => value.hasValue;

        public bool TryToGetValue(out T value) {
            if (this.value.TryToGetValue(out var currentValue)) {
                value = currentValue;
                return true;
            }

            value = default;
            return false;
        }

        public Maybe<T> Value {
            get => value;
            set => Set(value);
        }

        public void Bind(Action<Maybe<T>> handler) {
            ChangedValue += handler;
            handler(Value);
        }

        public void Unbind(Action<Maybe<T>> handler) {
            ChangedValue += handler;
        }

        protected virtual void Set(Maybe<T> maybeNewValue) {
            var currentHasValue = value.HasValue;
            var nextHasValue = maybeNewValue.HasValue;

            if (currentHasValue && nextHasValue) {
                if (maybeNewValue.TryToGetValue(out var nextValue) && value.TryToGetValue(out var currentValue)) {
                    bool equals = MyEquals(currentValue, nextValue);
                    if (!equals)
                        ChangeValue();
                }
            } else if (currentHasValue) {
                ChangeValue();
            } else if (nextHasValue) {
                ChangeValue();
            }

            void ChangeValue() {
                value = maybeNewValue;
                ChangedValue?.Invoke(value);
                Changed?.Invoke();
            }
        }

        bool MyEquals(T currentValue, T nextValue) {
            return isValueType ? currentValue.Equals(nextValue) : ReferenceEquals(currentValue, nextValue);
        }

        public void SetEmpty() {
            Set(Maybe<T>.Empty);
        }

        public void ForceUpdate() {
            ChangedValue?.Invoke(value);
            Changed?.Invoke();
        }

        public void SilentSet(T value) {
            this.value = value;
        }

        public bool Equals(Maybe<T> other) {
            var currentHasValue = value.HasValue;
            var nextHasValue = other.HasValue;
            
            if (currentHasValue && nextHasValue) {
                if (other.TryToGetValue(out var nextValue) && value.TryToGetValue(out var currentValue)) {
                    bool equals = MyEquals(currentValue, nextValue);
                    return equals;
                }
            } else if (currentHasValue) {
                return false;
            } else if (nextHasValue) {
                return false;
            }

            return true;
        }

        public bool Equals(T other) {
            if (value.TryToGetValue(out var currentValue)) {
                return MyEquals(currentValue, other);
            }

            return Equals(other, default(T));
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}