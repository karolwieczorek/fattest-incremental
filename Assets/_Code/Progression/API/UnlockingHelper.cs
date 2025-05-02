using System.Linq;
using FattestInc.Economy.API;
using Hypnagogia.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace FattestInc.Progression.API {
    [UsedImplicitly]
    public class UnlockingHelper {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        
        public bool CanBeShown(string factoryId) {
            var unlockingData =
                economyReferencer.UnlockingFactoriesData.LevelsList.FirstOrDefault(
                    x => x.IsShowType && x.factoryToUnlockId == factoryId);

            if (unlockingData == null) {
                Debug.LogWarning($"Missing Unlocking Data. {factoryId}");
                return true;
            }

            return AreAllConditionsMet(unlockingData);
        }

        public UnlockingFactoriesData.UnlockingData GetUnlockingData(string factoryId) {
            var unlockingData =
                economyReferencer.UnlockingFactoriesData.LevelsList.FirstOrDefault(
                    x => x.IsUnlockType && x.factoryToUnlockId == factoryId);
            return unlockingData;
        }

        public bool CanBeUnlocked(string factoryId) {
            var unlockingData =
                economyReferencer.UnlockingFactoriesData.LevelsList.FirstOrDefault(
                    x => x.IsUnlockType && x.factoryToUnlockId == factoryId);

            if (unlockingData == null) {
                Debug.LogWarning($"Missing Unlocking Data. {factoryId}");
                return true;
            }

            var areAllConditionsMet = AreAllConditionsMet(unlockingData);
            return areAllConditionsMet;
        }

        bool AreAllConditionsMet(UnlockingFactoriesData.UnlockingData unlockingData) {
            if (economyDataStore.CurrentTotalAmount.Value >= unlockingData.value == false)
                return false;
            if (economyDataStore.CurrentAmountPerSecond.Value >= unlockingData.valuePerSecond == false)
                return false;
            if (IsFactoryConditionMet(unlockingData.factory1Id, unlockingData.factory1Level) == false)
                return false;
            if (IsFactoryConditionMet(unlockingData.factory2Id, unlockingData.factory2Level) == false)
                return false;
            if (IsFactoryConditionMet(unlockingData.factory3Id, unlockingData.factory3Level) == false)
                return false;
            return true;
        }

        bool IsFactoryConditionMet(string factoryId, int factoryLevel) {
            if (string.IsNullOrEmpty(factoryId))
                return true;
            var (id, factory) = economyDataStore.ResourceFactories.FirstOrDefault(x => x.Key == factoryId);
            return factory != null && factory.Level >= factoryLevel;
        }
    }
}