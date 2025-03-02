using System.Collections.Generic;
using Hypnagogia.Utils;

namespace FattestInc {
    public class EconomyDataStore : HDataStore {
        public Observable<int> CurrentCalories { get; private set; } = new();
        readonly Dictionary<string, ResourceFactory> resourceFactories = new();

        public IReadOnlyDictionary<string, ResourceFactory> ResourceFactories => resourceFactories;

        public void AddOrUpgradeFactory(string factoryId, int i) {
            if (resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory.Upgrade(i);
            }
            else {
                resourceFactories.Add(factoryId, new ResourceFactory(i));
            }
        }
    }
}