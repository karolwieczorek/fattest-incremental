using System;

namespace Hypnagogia.Utils {
    public interface IReadOnlyObservable<out T> {
        public T Value { get; }

        public event Action<T> ChangedValue;
        public event Action Changed;
        void Bind(Action<T> handler);
        void Unbind(Action<T> handler);
    }
}