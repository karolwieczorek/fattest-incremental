using System.Collections.Generic;
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

        // Factories not yet unlocked. Unlocking is permanent, so once this is empty the refresh is free.
        readonly List<IFactoryData> lockedFactories = new();

        protected override void SystemStart() {
            lockedFactories.AddRange(factoriesReferencer.Factories);
            UnlockOrShowFactoryIfApplicable();
            economyDataStore.FactoryUpgradedEvent += UnlockOrShowFactoryIfApplicable;
            economyDataStore.TotalEarned.Changed += UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentAmountPerSecond.Changed += UnlockOrShowFactoryIfApplicable;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= UnlockOrShowFactoryIfApplicable;
            economyDataStore.TotalEarned.Changed -= UnlockOrShowFactoryIfApplicable;
            economyDataStore.CurrentAmountPerSecond.Changed -= UnlockOrShowFactoryIfApplicable;
        }

        void UnlockOrShowFactoryIfApplicable() {
            for (var i = lockedFactories.Count - 1; i >= 0; i--) {
                var factoryId = lockedFactories[i].FactoryId;
                // Runtime factories are created by their views; check again on the next refresh.
                if (economyDataStore.ResourceFactories.ContainsKey(factoryId) == false)
                    continue;

                var isFactoryUnlocked = economyDataStore.IsFactoryUnlocked(factoryId, out var factoryLevel);
                if (isFactoryUnlocked) {
                    lockedFactories.RemoveAt(i);
                    continue;
                }

                if (factoryLevel > 0) {
                    economyDataStore.MakeSureFactoryUnlocked(factoryId);
                    lockedFactories.RemoveAt(i);
                    continue;
                }

                if (unlockingHelper.CanBeUnlocked(factoryId)) {
                    economyDataStore.UnlockFactory(factoryId);
                    lockedFactories.RemoveAt(i);
                    continue;
                }

                if (economyDataStore.IsFactoryShown(factoryId) == false && unlockingHelper.CanBeShown(factoryId))
                    economyDataStore.ShowFactory(factoryId);
            }
        }

        public void Tick() {
            var deltaTime = Time.deltaTime;
            resourceFactoriesHelper.TickAllFactories(deltaTime);
        }
    }
}
