using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc.Economy.Implementation {
    public class ResourceFactoriesSystem : HSystem, ITickable {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] ResourceFactoriesHelper resourceFactoriesHelper;
        [HInject] UnlockingHelper unlockingHelper;

        protected override void SystemStart() {
            UnlockOrShowFactoryIfApplicable();
            economyDataStore.FactoryUpgradedEvent += UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentTotalAmount.Changed += UnlockOrShowFactoryIfApplicable;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentTotalAmount.Changed -= UnlockOrShowFactoryIfApplicable;
        }

        void UnlockOrShowFactoryIfApplicable() {
            Debug.Log("Refresh unlocking");
            var factoriesCopy = factoriesReferencer.Factories.ToList();
            foreach (var factory in factoriesCopy) {
                if (economyDataStore.ResourceFactories.ContainsKey(factory.FactoryId) == false) {
                    // Debug.LogWarning($"Factory {factory.FactoryId} not yet created. Continue");
                    continue;
                }

                // Debug.Log($"{factory.FactoryId}");
                var isFactoryUnlocked = economyDataStore.IsFactoryUnlocked(factory.FactoryId, out var factoryLevel);
                if (isFactoryUnlocked)
                    continue;

                if (factoryLevel > 0) {
                    economyDataStore.MakeSureFactoryUnlocked(factory.FactoryId);
                    continue;
                }

                if (unlockingHelper.CanBeUnlocked(factory.FactoryId)) {
                    economyDataStore.UnlockFactory(factory.FactoryId);
                    continue;
                }

                // if (economyDataStore.IsFactoryShown(factory.FactoryId)) {
                //     
                //     continue;
                // }

                if (unlockingHelper.CanBeShown(factory.FactoryId))
                    economyDataStore.ShowFactory(factory.FactoryId);
            }
        }

        public void Tick() {
            var deltaTime = Time.deltaTime;
            resourceFactoriesHelper.TickAllFactories(deltaTime);
        }
    }
}