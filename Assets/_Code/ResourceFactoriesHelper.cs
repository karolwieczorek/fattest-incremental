using Hypnagogia.Utils;

namespace FattestInc {
    public class ResourceFactoriesHelper {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;

        public void TickAllFactories(float deltaTime) {
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                factory.Tick(deltaTime, out var produced);
                if (produced > 0) {
                    economyDataStore.CurrentTotalAmount.Value += (ulong) produced;
                }
            }
        }
    }
}