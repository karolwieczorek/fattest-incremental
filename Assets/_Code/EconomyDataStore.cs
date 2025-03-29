using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;

namespace FattestInc {
    public class EconomyDataStore : HDataStore {
        [ShowInInspector] public Observable<int> CurrentTotalAmount { get; private set; } = new();
        [ShowInInspector] readonly Dictionary<string, ResourceFactory> resourceFactories = new();

        public IReadOnlyDictionary<string, ResourceFactory> ResourceFactories => resourceFactories;

        public ResourceFactory AddOrUpgradeFactory(string factoryId, int i) {
            if (resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory.Upgrade(i);
                return factory;
            }

            factory = new ResourceFactory(i);
            resourceFactories.Add(factoryId, factory);
            return factory;
        }
    }
}