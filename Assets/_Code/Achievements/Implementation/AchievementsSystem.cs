using System;
using FattestInc.Achievements.API;
using FattestInc.Economy.API;
using Hypnagogia.Utils;

namespace FattestInc.Achievements.Implementation {
    // Unlocks achievements when their conditions are met and saves right away.
    public class AchievementsSystem : HSystem {
        [HInject] AchievementsDataStore achievementsDataStore;
        [HInject] AchievementsReferencer achievementsReferencer;
        [HInject] EconomyDataStore economyDataStore;
        [HInject] EconomyReferencer economyReferencer;
        [HInject] FactoriesReferencer factoriesReferencer;
        [HInject] SaveHelper saveHelper;

        Func<string, int> getFactoryLevel;

        protected override void SystemStart() {
            getFactoryLevel = GetFactoryLevel;
            CheckAchievements();
            economyDataStore.FactoryUpgradedEvent += CheckAchievements;
            economyDataStore.CurrentAmountPerSecond.Changed += CheckAchievements;
        }

        protected override void SystemStop() {
            economyDataStore.FactoryUpgradedEvent -= CheckAchievements;
            economyDataStore.CurrentAmountPerSecond.Changed -= CheckAchievements;
        }

        void CheckAchievements() {
            var tiers = economyReferencer.ProgressionLevelsData.LevelsList;
            var lastTierThreshold = tiers.Count > 0 ? tiers[^1].value : 0;
            var anyUnlocked = false;
            foreach (var achievement in achievementsReferencer.Achievements) {
                if (achievementsDataStore.IsUnlocked(achievement.id))
                    continue;
                if (AchievementConditions.IsMet(achievement.condition, factoriesReferencer.Factories, getFactoryLevel,
                        economyDataStore.CurrentAmountPerSecond.Value, lastTierThreshold))
                    anyUnlocked |= achievementsDataStore.TryUnlock(achievement.id);
            }

            if (anyUnlocked)
                saveHelper.SaveGame(silentMode: true);
        }

        int GetFactoryLevel(string factoryId) {
            return economyDataStore.ResourceFactories.TryGetValue(factoryId, out var factory) ? factory.Level : 0;
        }
    }
}
