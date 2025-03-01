namespace Hypnagogia.Utils {
    public interface IMaybe<T> {
        bool HasValue { get; }
        bool TryToGetValue(out T value);
    }
}