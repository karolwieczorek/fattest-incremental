using System;
using UnityEngine;

namespace FattestInc.Economy.API {
    // In-memory formula factory (no asset). FormulaFactoryData uses the same math, so the simulator and
    // the auto-tuner can try values without touching assets.
    [Serializable]
    public class FormulaFactory : IFactoryData {
        public string factoryId;
        public string factoryName;
        public FactoryType factoryType = FactoryType.Idle;
        public int startingLevel;
        public int maxLevel = 1000;
        public ulong baseCost = 10;
        public double costMultiplier = 1.15;
        public ulong baseOutput = 1;
        public bool linearOutputByLevel = true;
        public int durationSeconds = 1;

        public string FactoryId => factoryId;
        public string FactoryName => factoryName;
        public Sprite Icon => null;
        public FactoryType FactoryType => factoryType;
        public int StartingLevel => startingLevel;

        public ulong GetCostForNextLevel(int factoryLevel) => GetCostForLevel(factoryLevel + 1);
        public ulong GetCostForLevel(int factoryLevel) => GetCost(baseCost, costMultiplier, factoryLevel);
        public ulong GetValueForLevel(int factoryLevel) => GetValue(baseOutput, linearOutputByLevel, factoryLevel);
        public int GetDurationForLevel(int factoryLevel) => GetDuration(factoryType, durationSeconds, factoryLevel);
        public bool IsLastLevel(int factoryLevel) => factoryLevel >= maxLevel;
        public int GetLastLevel() => maxLevel;
        public bool HasNextLevel(int factoryLevel) => factoryLevel < maxLevel;

        public static ulong GetCost(ulong baseCost, double costMultiplier, int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;
            // Cost at level N: baseCost * costMultiplier^(N-1)
            double cost = baseCost * Math.Pow(costMultiplier, factoryLevel - 1);
            // Tolerance so floating point noise (100 * 1.1 = 110.00000000000001) doesn't round up to 111.
            return (ulong)Math.Ceiling(cost * (1 - 1e-12));
        }

        public static ulong GetValue(ulong baseOutput, bool linearOutputByLevel, int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;
            // Output scales linearly with level, or stays at base output
            return linearOutputByLevel ? baseOutput * (ulong)factoryLevel : baseOutput;
        }

        public static int GetDuration(FactoryType factoryType, int durationSeconds, int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;
            if (factoryType == FactoryType.Clicker)
                return 0; // Clickers are instant (handled separately)
            return durationSeconds;
        }
    }
}
