using System;
using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class EconomyDataStore : HDataStore {
        [ShowInInspector] public Observable<int> CurrentTotalAmount { get; private set; } = new();
        [ShowInInspector] readonly Dictionary<string, ResourceFactory> resourceFactories = new();

        public IReadOnlyDictionary<string, ResourceFactory> ResourceFactories => resourceFactories;
        public event Action FactoryUpgradedEvent;

        public ResourceFactory AddOrUpgradeFactory(string factoryId, int i) {
            if (resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory.Upgrade(i);
            } else {
                factory = new ResourceFactory(i);
                resourceFactories.Add(factoryId, factory);
            }
            
            Debug.Log("Upgraded");
            FactoryUpgradedEvent?.Invoke();
            return factory;
        }
    }
}