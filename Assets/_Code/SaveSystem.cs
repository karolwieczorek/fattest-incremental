using FattestInc.Economy.API;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class SaveSystem : HSystem {
        [HInject] EconomyDataStore economyDataStore;

        protected override void SystemStart() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
            ApplicationQuittingObserver.GameQuitEvent += OnGameQuit;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= OnFactoryUpgraded;
            ApplicationQuittingObserver.GameQuitEvent -= OnGameQuit;
        }

        void OnFactoryUpgraded() {
            Debug.Log("Factory upgraded. Save");
        }

        void OnGameQuit() {
            Debug.Log("Game About to quit. Save");
        }
    }
}