using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc {
    public class ResourceFactoriesSystem : HSystem, ITickable {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] ResourceFactoriesHelper resourceFactoriesHelper;

        protected override void SystemStart() {
            base.SystemStart();
            economyDataStore.CurrentTotalAmount.Value = (ulong)economyReferencer.StartingValue;
            foreach (var factoryLevelsData in factoriesReferencer.Factories) {
                if (factoryLevelsData.StartingLevel > 0) {
                    economyDataStore.AddOrUpgradeFactory(factoryLevelsData, factoryLevelsData.StartingLevel);
                }
            }
        }

        public void Tick() {
            var deltaTime = Time.deltaTime;
            resourceFactoriesHelper.TickAllFactories(deltaTime);
        }
    }
}