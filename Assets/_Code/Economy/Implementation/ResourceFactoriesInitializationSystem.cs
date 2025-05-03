using FattestInc.Economy.API;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Economy.Implementation {
    public class ResourceFactoriesInitializationSystem : HSystem {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] SaveHelper saveHelper;

        protected override void SystemStart() {
            base.SystemStart();
            economyDataStore.Clear();
            var gameLoaded = saveHelper.TryLoadGame();
            if (!gameLoaded) {
                CreateFreshFactoriesState();
            }
        }

        void CreateFreshFactoriesState() {
            economyDataStore.CurrentTotalAmount.Value = (ulong) economyReferencer.StartingValue;
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                if (factoryLevelsData.StartingLevel > 0) {
                    economyDataStore.AddOrUpgradeFactory(factoryLevelsData, factoryLevelsData.StartingLevel);
                    economyDataStore.ShowFactory(factoryLevelsData.FactoryId);
                }
            }

            Debug.Log("Fresh factories setup created.");
        }
    }
}