using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc.Economy.Implementation {
    public class ResourceFactoriesSystem : HSystem, ITickable {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] ResourceFactoriesHelper resourceFactoriesHelper;
        [HInject] UnlockingHelper unlockingHelper;

        protected override void SystemStart() {
            base.SystemStart();
            economyDataStore.CurrentTotalAmount.Value = (ulong)economyReferencer.StartingValue;
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                if (factoryLevelsData.StartingLevel > 0) {
                    economyDataStore.AddOrUpgradeFactory(factoryLevelsData, factoryLevelsData.StartingLevel);
                    economyDataStore.ShowFactory(factoryLevelsData.FactoryId);
                }
            }
            
            economyDataStore.FactoryUpgradedEvent += UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentTotalAmount.Changed += UnlockOrShowFactoryIfApplicable;
        }

        protected override void SystemStop() {
            base.SystemStop();
            economyDataStore.FactoryUpgradedEvent -= UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentTotalAmount.Changed -= UnlockOrShowFactoryIfApplicable;
        }

        void UnlockOrShowFactoryIfApplicable() {
            foreach (var factory in factoriesReferencer.Factories) {
                // Debug.Log($"{factory.FactoryId}");
                if (economyDataStore.IsFactoryUnlocked(factory.FactoryId))
                    continue;

                if (economyDataStore.IsFactoryShown(factory.FactoryId)) {
                    if (unlockingHelper.CanBeUnlocked(factory.FactoryId))
                        economyDataStore.UnlockFactory(factory.FactoryId);
                    continue;
                }

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