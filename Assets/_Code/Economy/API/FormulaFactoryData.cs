using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Economy.API {
    [CreateAssetMenu(fileName = "FormulaFactoryData", menuName = "FattestInc/Factories/Formula Factory Data")]
    public class FormulaFactoryData : ScriptableObject, IFactoryData {
        [FoldoutGroup("Info")] [SerializeField] string factoryId;
        [FoldoutGroup("Info")] [SerializeField] string factoryName;
        [FoldoutGroup("Info")] [SerializeField] int startingLevel = 0;
        [FoldoutGroup("Info")] [SerializeField] Sprite icon;
        [FoldoutGroup("Info")] [SerializeField] FactoryType factoryType = FactoryType.Idle;
        [FoldoutGroup("Info")] [SerializeField] int maxLevel = 1000;

        [Title("Formulas")] [MinValue(1)] [SerializeField] ulong baseCost = 10;
        [SerializeField] double costMultiplier = 1.15;

        [MinValue(0)] [SerializeField] ulong baseOutput = 1;
        [Tooltip("Idle factories use this as output per tick; Clicker uses as output per click. Linear scaling by level.")]
        [SerializeField] bool linearOutputByLevel = true;

        [Title("Timing")] [Min(0)] [SerializeField] int durationSeconds = 1;

        public string FactoryId => factoryId;
        public string FactoryName => factoryName;
        public Sprite Icon => icon;
        public FactoryType FactoryType => factoryType;
        public int StartingLevel => startingLevel;

        public ulong GetCostForNextLevel(int factoryLevel) {
            var targetLevel = factoryLevel + 1;
            return GetCostForLevel(targetLevel);
        }

        public ulong GetCostForLevel(int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;
            // Cost at level N: baseCost * costMultiplier^(N-1)
            double cost = baseCost * Math.Pow(costMultiplier, factoryLevel - 1);
            return (ulong)Math.Ceiling(cost);
        }

        public ulong GetValueForLevel(int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;

            if (linearOutputByLevel) {
                // Output scales linearly with level
                return (ulong)(baseOutput * (ulong)factoryLevel);
            }

            // Fallback to base output only
            return baseOutput;
        }

        public int GetDurationForLevel(int factoryLevel) {
            if (factoryLevel <= 0)
                return 0;

            if (factoryType == FactoryType.Clicker)
                return 0; // Clickers are instant (handled separately)

            return durationSeconds;
        }

        public bool IsLastLevel(int factoryLevel) {
            return factoryLevel >= maxLevel;
        }

        public int GetLastLevel() {
            return maxLevel;
        }

        public bool HasNextLevel(int factoryLevel) {
            return factoryLevel < maxLevel;
        }
    }
} 