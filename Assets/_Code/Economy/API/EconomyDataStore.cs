using System;
using System.Collections.Generic;
using System.Linq;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Economy.API {
    public class EconomyDataStore : HDataStore {
        [ShowInInspector] public Observable<ulong> CurrentTotalAmount { get; private set; } = new();
        [ShowInInspector] public Observable<float> CurrentAmountPerSecond { get; private set; } = new();
        [ShowInInspector] readonly Dictionary<string, ResourceFactory> resourceFactories = new();

        public IReadOnlyDictionary<string, ResourceFactory> ResourceFactories => resourceFactories;
        public event Action FactoryUpgradedEvent;

        
        public ResourceFactory GetOrInitFactory(FactoryLevelsData factoryLevelsData) {
            var factoryId = factoryLevelsData.FactoryId;
            if (!resourceFactories.TryGetValue(factoryId, out var factory))
                factory = InitResourceFactory();
            return factory;

            ResourceFactory InitResourceFactory() {
                factory = new ResourceFactory(factoryLevelsData.FactoryType);
                resourceFactories.Add(factoryId, factory);
                var level = 0;
                var value = factoryLevelsData.GetValueForLevel(level);
                // var cost = factoryLevelsData.GetCostForNextLevel(factory.Level);
                var duration = factoryLevelsData.GetDurationForLevel(level);
                factory.Upgrade(level, value, duration);
                return factory;
            }
        }
        
        public ResourceFactory AddOrUpgradeFactory(FactoryLevelsData factoryLevelsData, int i) {
            var factoryId = factoryLevelsData.FactoryId;
            
            if (!resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory = new ResourceFactory(factoryLevelsData.FactoryType);
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

        public ResourceFactory LoadFactory(FactoryLevelsData factoryLevelsData, int level) {
            var factoryId = factoryLevelsData.FactoryId;
            
            if (!resourceFactories.TryGetValue(factoryId, out var factory)) {
                factory = new ResourceFactory(factoryLevelsData.FactoryType);
                resourceFactories.Add(factoryId, factory);
            }
            
            var value = factoryLevelsData.GetValueForLevel(level);
            var duration = factoryLevelsData.GetDurationForLevel(level);
            factory.Upgrade(level, value, duration);

            FactoryUpgradedEvent?.Invoke();
            return factory;
        }

        public bool HasEnoughMoney(ulong cost) {
            return CurrentTotalAmount.Value >= cost;
        }

        public bool TryBuy(ulong cost) {
            if (HasEnoughMoney(cost)) {
                CurrentTotalAmount.Value -= cost;
                return true;
            }
            return false;
        }

        public bool IsFactoryUnlocked(string factoryId, out int factoryLevel) {
            if (ResourceFactories.ContainsKey(factoryId) == false) {
                Debug.LogWarning($"Missing factory: {factoryId}");
                factoryLevel = 0;
                return false;
            }
            var factory = ResourceFactories[factoryId];
            factoryLevel = factory.Level;
            return factory.State == FactoryState.Unlocked;
        }

        public bool IsFactoryShown(string factoryId) {
            return ResourceFactories.ContainsKey(factoryId) && ResourceFactories[factoryId].State == FactoryState.Shown;
        }

        public void UnlockFactory(string factoryId) {
            var (key, factory) = ResourceFactories.FirstOrDefault(x => x.Key == factoryId);
            if (factory == null) {
                Debug.LogError($"Could not unlock factory. Factory {factoryId} is missing");
                return;
            }

            factory.Unlock();
        }

        public void ShowFactory(string factoryId) {
            var (key, factory) = ResourceFactories.FirstOrDefault(x => x.Key == factoryId);
            if (factory == null) {
                Debug.LogError($"Could not unlock factory. Factory {factoryId} is missing.");
                return;
            }

            factory.Show();
        }

        public void MakeSureFactoryUnlocked(string factoryId) {
            if (ResourceFactories.ContainsKey(factoryId) == false) {
                Debug.LogWarning($"Missing factory: {factoryId}");
                return;
            }

            var factory = ResourceFactories[factoryId];
            if (factory.State != FactoryState.Unlocked)
                factory.Unlock();
        }
    }
}