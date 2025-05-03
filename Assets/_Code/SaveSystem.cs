using FattestInc.Economy.API;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc {
    public class SaveSystem : HSystem {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] SaveHelper saveHelper;

        protected override void SystemStart() {
            economyDataStore.FactoryUpgradedEvent += OnFactoryUpgraded;
            ApplicationQuittingObserver.GameQuitEvent += OnGameQuit;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= OnFactoryUpgraded;
            ApplicationQuittingObserver.GameQuitEvent -= OnGameQuit;
        }

        void OnFactoryUpgraded() {
            saveHelper.SaveGame(silentMode: true);
        }

        void OnGameQuit() {
            saveHelper.SaveGame();
        }
    }
}