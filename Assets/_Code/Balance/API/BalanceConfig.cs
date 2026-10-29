using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Balance.API {
    // Holds the balance settings and writes the tuned economy into the game data assets.
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "FattestInc/Balance Config")]
    public class BalanceConfig : ScriptableObject {
        public const string DefaultPath = "Assets/_Data/Balance/BalanceConfig.asset";

        [InlineProperty, HideLabel]
        [SerializeField] BalanceSettings settings = new();

        [Title("Data written by Apply")]
        [Tooltip("Factories in tier order. The first one is the starter and is left as is.")]
        [SerializeField] FactoriesReferencer factoriesReferencer;
        [SerializeField] UnlockingFactoriesData unlockingFactoriesData;
        [SerializeField] ProgressionLevelsData progressionLevelsData;

        [Title("Last run")]
        [MultiLineProperty(30), ReadOnly, HideLabel]
        [SerializeField] string lastReport;

        public BalanceSettings Settings => settings;

        public void Init(FactoriesReferencer factories, UnlockingFactoriesData unlocking, ProgressionLevelsData progression) {
            factoriesReferencer = factories;
            unlockingFactoriesData = unlocking;
            progressionLevelsData = progression;
        }

        public BalanceResult Tune() {
            var factories = GetFactoryAssets().Select(f => f.ToFormula()).ToList();
            var tierNames = progressionLevelsData.LevelsList.Select(l => l.name).ToList();
            return BalanceTuner.Tune(factories, settings, tierNames);
        }

        List<FormulaFactoryData> GetFactoryAssets() {
            var assets = new List<FormulaFactoryData>();
            foreach (var factory in factoriesReferencer.Factories) {
                if (factory is FormulaFactoryData formula)
                    assets.Add(formula);
                else
                    Debug.LogError($"Balance tool only supports {nameof(FormulaFactoryData)}; skipping {factory.FactoryId}.", this);
            }

            return assets;
        }

        public static string Describe(BalanceResult result) {
            var sb = new System.Text.StringBuilder();
            foreach (var warning in result.Warnings)
                sb.AppendLine($"WARNING: {warning}");
            sb.AppendLine("Factory       unlock at  base cost          base output     x/level  max");
            for (var i = 0; i < result.Factories.Count; i++) {
                var f = result.Factories[i];
                sb.AppendLine($"{f.factoryId,-13} {Simulation.API.SimulationReport.FormatTime(result.TargetUnlockTimes[i]),-10} {f.baseCost,-18:N0} {f.baseOutput,-15:N0} {f.costMultiplier,-8} {f.maxLevel}");
            }
            sb.AppendLine();
            sb.Append(result.Report.ToText());
            return sb.ToString();
        }

#if UNITY_EDITOR
        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        [Tooltip("Run the tuner and show the pacing report without changing any data.")]
        public string Preview() {
            lastReport = Describe(Tune());
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Balance preview\n{lastReport}", this);
            return lastReport;
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1), GUIColor(1f, 0.8f, 0.4f)]
        [Tooltip("Run the tuner and write factory formulas, unlock rows and progression tiers into the data assets.")]
        public string Apply() {
            var result = Tune();
            var assets = GetFactoryAssets();
            for (var i = 0; i < assets.Count; i++) {
                assets[i].ApplyFormula(result.Factories[i]);
                UnityEditor.EditorUtility.SetDirty(assets[i]);
            }

            unlockingFactoriesData.LevelsList.Clear();
            unlockingFactoriesData.LevelsList.AddRange(result.UnlockRows);
            UnityEditor.EditorUtility.SetDirty(unlockingFactoriesData);

            for (var i = 0; i < progressionLevelsData.LevelsList.Count; i++)
                progressionLevelsData.LevelsList[i].value = result.TierThresholds[i];
            UnityEditor.EditorUtility.SetDirty(progressionLevelsData);

            lastReport = Describe(result);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Balance applied\n{lastReport}", this);
            return lastReport;
        }

        // Entry points for the Unity CLI (`unity command script run`) and batch tools.
        public static string PreviewDefault() => Load().Preview();
        public static string ApplyDefault() => Load().Apply();

        static BalanceConfig Load() {
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<BalanceConfig>(DefaultPath);
            if (config == null)
                throw new System.InvalidOperationException($"No BalanceConfig at {DefaultPath}");
            return config;
        }
#endif
    }
}
