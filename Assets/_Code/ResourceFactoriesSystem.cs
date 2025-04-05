using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc {
    public class ResourceFactoriesSystem : HSystem, ITickable {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;

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
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                factory.Tick(Time.deltaTime, out var produced);
                if (produced > 0) {
                    economyDataStore.CurrentTotalAmount.Value += (ulong)produced;
                }
            }
        }
    }
}