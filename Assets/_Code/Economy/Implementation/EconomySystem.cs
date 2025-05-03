using FattestInc.Economy.API;
using Hypnagogia.Utils;

namespace FattestInc.Economy.Implementation {
    public class EconomySystem : HSystem {
        [HInject] EconomyDataStore economyDataStore;

        protected override void SystemStart() {
            RecalculateValuePerSecond();
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= OnFactoryUpgraded;
        }

        void OnFactoryUpgraded() {
            RecalculateValuePerSecond();
        }

        void RecalculateValuePerSecond() {
            var perSecond = 0f;
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                perSecond += factory.GetCurrentValuePerSecond();
            }

            economyDataStore.CurrentAmountPerSecond.Value = perSecond;
        }
    }
}