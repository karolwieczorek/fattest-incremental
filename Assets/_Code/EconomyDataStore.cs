using Hypnagogia.Utils;

namespace FattestInc {
    public class EconomyDataStore : HDataStore {
        public Observable<int> CurrentCalories { get; private set; } = new();
    }
}