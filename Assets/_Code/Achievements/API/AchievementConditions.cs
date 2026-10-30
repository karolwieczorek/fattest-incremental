using System;
using System.Collections.Generic;
using FattestInc.Economy.API;

namespace FattestInc.Achievements.API {
    // Pure condition checks, so tests and tools can use them without a scene.
    public static class AchievementConditions {
        public static bool IsMet(AchievementCondition condition, IReadOnlyList<IFactoryData> factories,
            Func<string, int> getFactoryLevel, double incomePerSecond, ulong lastTierThreshold) {
            return condition switch {
                AchievementCondition.ReachLastProgressionTier => lastTierThreshold > 0 && incomePerSecond >= lastTierThreshold,
                AchievementCondition.MaxAllFactories => AreAllFactoriesMaxed(factories, getFactoryLevel),
                _ => false,
            };
        }

        public static bool AreAllFactoriesMaxed(IReadOnlyList<IFactoryData> factories, Func<string, int> getFactoryLevel) {
            if (factories == null || factories.Count == 0)
                return false;
            foreach (var factory in factories) {
                if (factory.HasNextLevel(getFactoryLevel(factory.FactoryId)))
                    return false;
            }

            return true;
        }
    }
}
