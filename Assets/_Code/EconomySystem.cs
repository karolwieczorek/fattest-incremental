using Hypnagogia.Utils;

namespace FattestInc {
    public class EconomySystem : HSystem {
        [HInject] EconomyDataStore economyDataStore;
        
        protected override void SystemStart() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= OnFactoryUpgraded;
        }
        
        void OnFactoryUpgraded() {
            var perSecond = 0f;
            foreach (var (_, factory) in economyDataStore.ResourceFactories) {
                perSecond += factory.GetCurrentValuePerSecond();
            }

            economyDataStore.CurrentAmountPerSecond.Value = perSecond;
        }
    }
}