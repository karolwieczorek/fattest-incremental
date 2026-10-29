using System;

namespace FattestInc.Progression.API {
    // Shared by the game (UnlockingHelper) and EconomySimulator so both evaluate rules the same way.
    public static class UnlockConditions {
        public static bool AreMet(UnlockingFactoriesData.UnlockingData rule, ulong totalEarned, double amountPerSecond,
            Func<string, int> getFactoryLevel) {
            if (rule == null)
                return true;
            if (totalEarned < rule.value)
                return false;
            if (amountPerSecond < rule.valuePerSecond)
                return false;

            return IsFactoryRequirementMet(rule.factory1Id, rule.factory1Level, getFactoryLevel)
                && IsFactoryRequirementMet(rule.factory2Id, rule.factory2Level, getFactoryLevel)
                && IsFactoryRequirementMet(rule.factory3Id, rule.factory3Level, getFactoryLevel);
        }

        static bool IsFactoryRequirementMet(string factoryId, int requiredLevel, Func<string, int> getFactoryLevel) {
            if (string.IsNullOrEmpty(factoryId) || requiredLevel <= 0)
                return true;
            return getFactoryLevel(factoryId) >= requiredLevel;
        }
    }
}
