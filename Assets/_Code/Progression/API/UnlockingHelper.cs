using System;
using System.Collections.Generic;
using FattestInc.Economy.API;
using Hypnagogia.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace FattestInc.Progression.API {
    [UsedImplicitly]
    public class UnlockingHelper {
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;

        Dictionary<string, UnlockingFactoriesData.UnlockingData> showRules;
        Dictionary<string, UnlockingFactoriesData.UnlockingData> unlockRules;
        Func<string, int> getFactoryLevel;

        Dictionary<string, UnlockingFactoriesData.UnlockingData> ShowRules =>
            showRules ??= economyReferencer.UnlockingFactoriesData.GetShowRules();

        Dictionary<string, UnlockingFactoriesData.UnlockingData> UnlockRules =>
            unlockRules ??= economyReferencer.UnlockingFactoriesData.GetUnlockRules();

        public bool CanBeShown(string factoryId) {
            if (ShowRules.TryGetValue(factoryId, out var unlockingData) == false) {
                Debug.LogWarning($"Missing Unlocking Data. {factoryId}");
                return true;
            }

            return AreAllConditionsMet(unlockingData);
        }

        public UnlockingFactoriesData.UnlockingData GetUnlockingData(string factoryId) {
            UnlockRules.TryGetValue(factoryId, out var unlockingData);
            return unlockingData;
        }

        public bool CanBeUnlocked(string factoryId) {
            if (UnlockRules.TryGetValue(factoryId, out var unlockingData) == false) {
                Debug.LogWarning($"Missing Unlocking Data. {factoryId}");
                return true;
            }

            return AreAllConditionsMet(unlockingData);
        }

        bool AreAllConditionsMet(UnlockingFactoriesData.UnlockingData unlockingData) {
            getFactoryLevel ??= GetFactoryLevel;
            return UnlockConditions.AreMet(unlockingData, economyDataStore.TotalEarned.Value,
                economyDataStore.CurrentAmountPerSecond.Value, getFactoryLevel);
        }

        int GetFactoryLevel(string factoryId) {
            return economyDataStore.ResourceFactories.TryGetValue(factoryId, out var factory) ? factory.Level : 0;
        }
    }
}