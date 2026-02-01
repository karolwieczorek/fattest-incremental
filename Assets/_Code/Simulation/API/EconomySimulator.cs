using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;

namespace FattestInc.Simulation.API {
    public class EconomySimulator {
        public SimulationResult Run(IReadOnlyList<IFactoryData> factories,
            int durationSeconds,
            double manualClicksPerSecond,
            bool buyOnlyOnePerSecond = true,
            double startingEnergy = 0d,
            UnlockingFactoriesData unlockingData = null) {
            if (factories == null || factories.Count == 0)
                return new SimulationResult(new List<Snapshot>());

            // Initialize owned levels from StartingLevel (so clickers at level 1 produce immediately)
            var ownedLevels = factories.ToDictionary(f => f.FactoryId, f => Math.Max(0, f.StartingLevel));
            double energy = startingEnergy;
            double totalEarned = energy;

            // Unlock rules map
            var unlockRules = BuildUnlockRules(unlockingData);
            var unlockedFactories = new HashSet<string>();
            // Initial unlocked set: any factory with no rule, or with StartingLevel > 0
            foreach (var f in factories) {
                if (ownedLevels[f.FactoryId] > 0 || !unlockRules.ContainsKey(f.FactoryId))
                    unlockedFactories.Add(f.FactoryId);
            }

            var snapshots = new List<Snapshot>(durationSeconds);

            for (int t = 0; t < durationSeconds; t++) {
                // Produce energy for this second
                var produced = ProduceEnergyForSecond(factories, ownedLevels, manualClicksPerSecond);
                energy += produced;
                totalEarned += produced;

                // Snapshot BEFORE purchasing (to match the provided pseudocode)
                var eps = CalculateEnergyPerSecond(factories, ownedLevels, manualClicksPerSecond);
                snapshots.Add(CreateSnapshot(t, energy, eps, totalEarned, factories, ownedLevels));

                // Update unlocked state based on current EPS and owned levels
                UpdateUnlockedFactories(factories, unlockRules, unlockedFactories, ownedLevels, eps);

                // Purchase logic: iterate factories in reversed input order and buy first affordable among unlocked
                TryPurchase(factories, ref energy, ownedLevels, buyOnlyOnePerSecond, unlockedFactories);
            }

            return new SimulationResult(snapshots);
        }

        static Dictionary<string, UnlockingFactoriesData.UnlockingData> BuildUnlockRules(UnlockingFactoriesData unlockingData) {
            var result = new Dictionary<string, UnlockingFactoriesData.UnlockingData>();
            if (unlockingData == null || unlockingData.LevelsList == null)
                return result;
            foreach (var entry in unlockingData.LevelsList) {
                if (entry != null && entry.IsUnlockType && !string.IsNullOrEmpty(entry.factoryToUnlockId)) {
                    // Last wins if duplicates
                    result[entry.factoryToUnlockId] = entry;
                }
            }
            return result;
        }

        static bool AreUnlockConditionsMet(UnlockingFactoriesData.UnlockingData rule,
            IReadOnlyDictionary<string, int> ownedLevels,
            double currentEps) {
            if (rule == null)
                return true;

            if (rule.valuePerSecond > 0 && currentEps < rule.valuePerSecond)
                return false;

            bool FactoryReq(string id, int lvl) {
                if (string.IsNullOrEmpty(id) || lvl <= 0)
                    return true;
                return ownedLevels.TryGetValue(id, out var have) && have >= lvl;
            }

            if (!FactoryReq(rule.factory1Id, rule.factory1Level)) return false;
            if (!FactoryReq(rule.factory2Id, rule.factory2Level)) return false;
            if (!FactoryReq(rule.factory3Id, rule.factory3Level)) return false;

            return true;
        }

        static void UpdateUnlockedFactories(IReadOnlyList<IFactoryData> factories,
            IReadOnlyDictionary<string, UnlockingFactoriesData.UnlockingData> unlockRules,
            ISet<string> unlocked,
            IReadOnlyDictionary<string, int> ownedLevels,
            double currentEps) {
            foreach (var f in factories) {
                if (unlocked.Contains(f.FactoryId))
                    continue;
                if (unlockRules.TryGetValue(f.FactoryId, out var rule)) {
                    if (AreUnlockConditionsMet(rule, ownedLevels, currentEps))
                        unlocked.Add(f.FactoryId);
                } else {
                    // No rule -> unlocked
                    unlocked.Add(f.FactoryId);
                }
            }
        }

        static double ProduceEnergyForSecond(IReadOnlyList<IFactoryData> factories,
            Dictionary<string, int> ownedLevels,
            double manualClicksPerSecond) {
            double produced = 0d;
            foreach (var f in factories) {
                int level = ownedLevels[f.FactoryId];
                if (level <= 0)
                    continue;

                if (f.FactoryType == FactoryType.Clicker) {
                    var perClick = f.GetValueForLevel(level);
                    produced += perClick * manualClicksPerSecond;
                } else {
                    var valuePerTick = f.GetValueForLevel(level);
                    var duration = f.GetDurationForLevel(level);
                    var perSecond = duration > 0 ? (double)valuePerTick / duration : 0d;
                    produced += perSecond;
                }
            }

            return produced;
        }

        static double CalculateEnergyPerSecond(IReadOnlyList<IFactoryData> factories,
            Dictionary<string, int> ownedLevels,
            double manualClicksPerSecond) {
            double eps = 0d;
            foreach (var f in factories) {
                int level = ownedLevels[f.FactoryId];
                if (level <= 0)
                    continue;

                if (f.FactoryType == FactoryType.Clicker) {
                    var perClick = f.GetValueForLevel(level);
                    eps += perClick * manualClicksPerSecond;
                } else {
                    var valuePerTick = f.GetValueForLevel(level);
                    var duration = f.GetDurationForLevel(level);
                    var perSecond = duration > 0 ? (double)valuePerTick / duration : 0d;
                    eps += perSecond;
                }
            }

            return eps;
        }

        static void TryPurchase(IReadOnlyList<IFactoryData> factories,
            ref double energy,
            Dictionary<string, int> ownedLevels,
            bool buyOnlyOnePerSecond,
            ISet<string> unlockedFactories) {
            if (energy <= 0)
                return;

            if (buyOnlyOnePerSecond) {
                for (int i = factories.Count - 1; i >= 0; i--) {
                    var f = factories[i];
                    if (!unlockedFactories.Contains(f.FactoryId))
                        continue;
                    var currentLevel = ownedLevels[f.FactoryId];
                    if (!f.HasNextLevel(currentLevel))
                        continue;
                    var nextCost = f.GetCostForLevel(currentLevel + 1);
                    if (energy >= nextCost) {
                        energy -= nextCost;
                        ownedLevels[f.FactoryId] = currentLevel + 1;
                        break;
                    }
                }
            } else {
                for (int i = factories.Count - 1; i >= 0; i--) {
                    var f = factories[i];
                    if (!unlockedFactories.Contains(f.FactoryId))
                        continue;
                    while (true) {
                        var currentLevel = ownedLevels[f.FactoryId];
                        if (!f.HasNextLevel(currentLevel))
                            break;
                        var nextCost = f.GetCostForLevel(currentLevel + 1);
                        if (energy >= nextCost) {
                            energy -= nextCost;
                            ownedLevels[f.FactoryId] = currentLevel + 1;
                        } else {
                            break;
                        }
                    }
                }
            }
        }

        static Snapshot CreateSnapshot(int t, double energy, double eps, double total,
            IReadOnlyList<IFactoryData> factories,
            Dictionary<string, int> ownedLevels) {
            var levels = new List<FactoryLevelSnapshot>(factories.Count);
            foreach (var f in factories) {
                levels.Add(new FactoryLevelSnapshot(f.FactoryId, f.FactoryName, ownedLevels[f.FactoryId]));
            }

            return new Snapshot(t, energy, eps, total, levels);
        }
    }

    public readonly struct SimulationResult {
        public readonly IReadOnlyList<Snapshot> Snapshots;
        public SimulationResult(IReadOnlyList<Snapshot> snapshots) {
            Snapshots = snapshots;
        }
    }

    public readonly struct Snapshot {
        public readonly int TimeSeconds;
        public readonly double Energy;
        public readonly double EnergyTotalEarned ;
        public readonly double EnergyPerSecond;
        public readonly IReadOnlyList<FactoryLevelSnapshot> Levels;

        public Snapshot(int timeSeconds, double energy, double energyPerSecond, double totalEarned , IReadOnlyList<FactoryLevelSnapshot> levels) {
            TimeSeconds = timeSeconds;
            Energy = energy;
            EnergyPerSecond = energyPerSecond;
            EnergyTotalEarned = totalEarned;
            Levels = levels;
        }
    }

    public readonly struct FactoryLevelSnapshot {
        public readonly string FactoryId;
        public readonly string FactoryName;
        public readonly int Level;

        public FactoryLevelSnapshot(string factoryId, string factoryName, int level) {
            FactoryId = factoryId;
            FactoryName = factoryName;
            Level = level;
        }
    }
} 