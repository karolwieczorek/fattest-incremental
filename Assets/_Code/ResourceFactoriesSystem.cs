using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc {
    public class ResourceFactoriesSystem : HSystem, ITickable {
        [HInject] EconomyDataStore economyDataStore;

        public void Tick() {
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                factory.Tick(Time.deltaTime, out var produced);
                if (produced > 0) {
                    economyDataStore.CurrentTotalAmount.Value += produced;
                }
            }
        }
    }
}