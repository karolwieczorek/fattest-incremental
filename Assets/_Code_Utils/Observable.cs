using System;

namespace Hypnagogia.Utils {
    public class Observable<T> : IReadOnlyObservable<T> {
        T value;
        bool isValueType;

        public event Action<T> ChangedValue;
        public event Action Changed;

        public Observable(T value = default) {
            this.value = value;
            isValueType = typeof(T).IsValueType;
        }

        public T Value {
            get => value;
            set => Set(value);
        }

        public void Bind(Action<T> handler) {
            ChangedValue += handler;
            handler(Value);
        }

        public void Unbind(Action<T> handler) {
            ChangedValue -= handler;
        }

        protected virtual void Set(T newValue) {
            bool equals = isValueType ? value.Equals(newValue) : ReferenceEquals(value, newValue);
            if (!equals) {
                value = newValue;
                ChangedValue?.Invoke(value);
                Changed?.Invoke();
            }
        }

        public void ForceUpdate() {
            ChangedValue?.Invoke(value);
            Changed?.Invoke();
        }

        public void SilentSet(T value) {
            this.value = value;
        }

        public override string ToString() {
            return value.ToString();
        }
    }

    public class ObservableBool : Observable<bool> {
        public ObservableBool(bool value = default) : base(value) { }
    }

    public class ObservableFloat : Observable<float> {
        public ObservableFloat(float value = default) : base(value) { }
    }

    public class ObservableInt : Observable<int> {
        public ObservableInt(int value = default) : base(value) { }
    }
}