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

        public ResourceFactory AddOrUpgradeFactory(FactoryLevelsData factoryLevelsData, int i) {
            var factoryId = factoryLevelsData.FactoryId;
            
            if (!resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory = new ResourceFactory();
                resourceFactories.Add(factoryId, factory);
            }
            // else {
            //     factory.Upgrade(i);
            // }
            var nextLevel = factory.Level + i;
            var value = factoryLevelsData.GetValueForLevel(nextLevel);
            // var cost = factoryLevelsData.GetCostForNextLevel(factory.Level);
            var duration = factoryLevelsData.GetDurationForLevel(nextLevel);
            factory.Upgrade(nextLevel, value, duration);

            FactoryUpgradedEvent?.Invoke();
            return factory;
        }

        public bool HasEnoughMoney(int cost) {
            return CurrentTotalAmount.Value >= cost;
        }

        public bool TryBuy(int cost) {
            if (HasEnoughMoney(cost)) {
                CurrentTotalAmount.Value -= cost;
                return true;
            }
            return false;
        }
    }
}