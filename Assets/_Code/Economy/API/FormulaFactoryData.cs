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

        public ulong GetCostForNextLevel(int factoryLevel) => GetCostForLevel(factoryLevel + 1);
        public ulong GetCostForLevel(int factoryLevel) => FormulaFactory.GetCost(baseCost, costMultiplier, factoryLevel);
        public ulong GetValueForLevel(int factoryLevel) => FormulaFactory.GetValue(baseOutput, linearOutputByLevel, factoryLevel);
        public int GetDurationForLevel(int factoryLevel) => FormulaFactory.GetDuration(factoryType, durationSeconds, factoryLevel);
        public bool IsLastLevel(int factoryLevel) => factoryLevel >= maxLevel;
        public int GetLastLevel() => maxLevel;
        public bool HasNextLevel(int factoryLevel) => factoryLevel < maxLevel;

        public FormulaFactory ToFormula() {
            return new FormulaFactory {
                factoryId = factoryId,
                factoryName = factoryName,
                factoryType = factoryType,
                startingLevel = startingLevel,
                maxLevel = maxLevel,
                baseCost = baseCost,
                costMultiplier = costMultiplier,
                baseOutput = baseOutput,
                linearOutputByLevel = linearOutputByLevel,
                durationSeconds = durationSeconds,
            };
        }

        // Used by the balance tool. Identity fields (id, name, icon, type) are left alone.
        public void ApplyFormula(FormulaFactory formula) {
            startingLevel = formula.startingLevel;
            maxLevel = formula.maxLevel;
            baseCost = formula.baseCost;
            costMultiplier = formula.costMultiplier;
            baseOutput = formula.baseOutput;
            linearOutputByLevel = formula.linearOutputByLevel;
            durationSeconds = formula.durationSeconds;
        }
    }
}
