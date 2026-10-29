using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;

namespace FattestInc.Simulation.API {
    public enum PurchaseStrategy {
        // Buy the first affordable factory, starting from the last one in the list.
        HighestTierFirst,
        // Pick the upgrade with the lowest (time to afford + time to pay back) and wait for it.
        BestPayback,
    }

    public class SimulationOptions {
        public int DurationSeconds = 3600;
        public double ManualClicksPerSecond = 3d;
        public bool BuyOnlyOnePerSecond = true;
        public double StartingEnergy;
        public PurchaseStrategy Strategy = PurchaseStrategy.HighestTierFirst;
        // Per-second snapshots with every factory level. Turn off for fast batch runs (auto-tuning).
        public bool RecordSnapshots = true;
        // Stop as soon as this factory unlocks (auto-tuning only needs the unlock time).
        public string StopWhenUnlocked;
    }

    public class EconomySimulator {
        public SimulationResult Run(IReadOnlyList<IFactoryData> factories,
            int durationSeconds,
            double manualClicksPerSecond,
            bool buyOnlyOnePerSecond = true,
            double startingEnergy = 0d,
            UnlockingFactoriesData unlockingData = null,
            PurchaseStrategy strategy = PurchaseStrategy.HighestTierFirst) {
            var unlockRules = unlockingData != null
                ? unlockingData.GetUnlockRules()
                : new Dictionary<string, UnlockingFactoriesData.UnlockingData>();
            return Run(factories, unlockRules, new SimulationOptions {
                DurationSeconds = durationSeconds,
                ManualClicksPerSecond = manualClicksPerSecond,
                BuyOnlyOnePerSecond = buyOnlyOnePerSecond,
                StartingEnergy = startingEnergy,
                Strategy = strategy,
            });
        }

        public SimulationResult Run(IReadOnlyList<IFactoryData> factories,
            IReadOnlyDictionary<string, UnlockingFactoriesData.UnlockingData> unlockRules,
            SimulationOptions options) {
            if (factories == null || factories.Count == 0)
                return new SimulationResult(new List<Snapshot>());

            unlockRules ??= new Dictionary<string, UnlockingFactoriesData.UnlockingData>();
            var clicks = options.ManualClicksPerSecond;

            // Initialize owned levels from StartingLevel (so clickers at level 1 produce immediately)
            var ownedLevels = factories.ToDictionary(f => f.FactoryId, f => Math.Max(0, f.StartingLevel));
            double energy = options.StartingEnergy;
            double totalEarned = energy;

            Func<string, int> getFactoryLevel = id => ownedLevels.TryGetValue(id, out var level) ? level : 0;
            var unlockedFactories = new HashSet<string>();
            var unlockTimes = new Dictionary<string, int>();
            var purchases = new List<PurchaseEvent>();
            var epsBySecond = new List<double>(options.DurationSeconds);
            // Initial unlocked set: any factory with no rule, or with StartingLevel > 0
            foreach (var f in factories) {
                if (ownedLevels[f.FactoryId] > 0 || !unlockRules.ContainsKey(f.FactoryId)) {
                    unlockedFactories.Add(f.FactoryId);
                    unlockTimes[f.FactoryId] = 0;
                }
            }

            var snapshots = new List<Snapshot>(options.RecordSnapshots ? options.DurationSeconds : 0);

            for (int t = 0; t < options.DurationSeconds; t++) {
                // Produce energy for this second
                var eps = CalculateEnergyPerSecond(factories, ownedLevels, clicks);
                energy += eps;
                totalEarned += eps;

                // Snapshot BEFORE purchasing (to match the provided pseudocode)
                epsBySecond.Add(eps);
                if (options.RecordSnapshots)
                    snapshots.Add(CreateSnapshot(t, energy, eps, totalEarned, factories, ownedLevels));

                // Update unlocked state based on current EPS and owned levels
                UpdateUnlockedFactories(factories, unlockRules, unlockedFactories, unlockTimes, getFactoryLevel, eps, totalEarned, t);
                if (options.StopWhenUnlocked != null && unlockTimes.ContainsKey(options.StopWhenUnlocked))
                    break;

                if (options.Strategy == PurchaseStrategy.BestPayback)
                    TryPurchaseBestPayback(factories, ref energy, ownedLevels, options.BuyOnlyOnePerSecond, unlockedFactories, clicks, purchases, t);
                else
                    TryPurchase(factories, ref energy, ownedLevels, options.BuyOnlyOnePerSecond, unlockedFactories, purchases, t);
            }

            return new SimulationResult(snapshots, epsBySecond, unlockTimes, purchases);
        }

        static void UpdateUnlockedFactories(IReadOnlyList<IFactoryData> factories,
            IReadOnlyDictionary<string, UnlockingFactoriesData.UnlockingData> unlockRules,
            ISet<string> unlocked,
            IDictionary<string, int> unlockTimes,
            Func<string, int> getFactoryLevel,
            double currentEps,
            double totalEarned,
            int t) {
            foreach (var f in factories) {
                if (unlocked.Contains(f.FactoryId))
                    continue;
                unlockRules.TryGetValue(f.FactoryId, out var rule);
                // No rule -> unlocked
                if (UnlockConditions.AreMet(rule, (ulong)totalEarned, currentEps, getFactoryLevel)) {
                    unlocked.Add(f.FactoryId);
                    unlockTimes[f.FactoryId] = t;
                }
            }
        }

        public static double GetEnergyPerSecond(IFactoryData f, int level, double manualClicksPerSecond) {
            if (level <= 0)
                return 0d;

            var value = f.GetValueForLevel(level);
            if (f.FactoryType == FactoryType.Clicker)
                return value * manualClicksPerSecond;

            var duration = f.GetDurationForLevel(level);
            return duration > 0 ? (double)value / duration : 0d;
        }

        static double CalculateEnergyPerSecond(IReadOnlyList<IFactoryData> factories,
            Dictionary<string, int> ownedLevels,
            double manualClicksPerSecond) {
            double eps = 0d;
            foreach (var f in factories)
                eps += GetEnergyPerSecond(f, ownedLevels[f.FactoryId], manualClicksPerSecond);
            return eps;
        }

        static void TryPurchase(IReadOnlyList<IFactoryData> factories,
            ref double energy,
            Dictionary<string, int> ownedLevels,
            bool buyOnlyOnePerSecond,
            ISet<string> unlockedFactories,
            List<PurchaseEvent> purchases,
            int t) {
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
                        Buy(f, ref energy, ownedLevels, nextCost, purchases, t);
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
                            Buy(f, ref energy, ownedLevels, nextCost, purchases, t);
                        } else {
                            break;
                        }
                    }
                }
            }
        }

        static void TryPurchaseBestPayback(IReadOnlyList<IFactoryData> factories,
            ref double energy,
            Dictionary<string, int> ownedLevels,
            bool buyOnlyOnePerSecond,
            ISet<string> unlockedFactories,
            double manualClicksPerSecond,
            List<PurchaseEvent> purchases,
            int t) {
            while (true) {
                var eps = Math.Max(CalculateEnergyPerSecond(factories, ownedLevels, manualClicksPerSecond), 1e-9);
                IFactoryData best = null;
                ulong bestCost = 0;
                var bestScore = double.MaxValue;
                foreach (var f in factories) {
                    if (!unlockedFactories.Contains(f.FactoryId))
                        continue;
                    var level = ownedLevels[f.FactoryId];
                    if (!f.HasNextLevel(level))
                        continue;
                    var gain = GetEnergyPerSecond(f, level + 1, manualClicksPerSecond) - GetEnergyPerSecond(f, level, manualClicksPerSecond);
                    if (gain <= 0)
                        continue;
                    var cost = f.GetCostForLevel(level + 1);
                    var score = cost / eps + cost / gain;
                    if (score < bestScore) {
                        bestScore = score;
                        best = f;
                        bestCost = cost;
                    }
                }

                // Wait for the best option instead of buying a worse affordable one.
                if (best == null || energy < bestCost)
                    return;

                Buy(best, ref energy, ownedLevels, bestCost, purchases, t);
                if (buyOnlyOnePerSecond)
                    return;
            }
        }

        static void Buy(IFactoryData f, ref double energy, Dictionary<string, int> ownedLevels, ulong cost,
            List<PurchaseEvent> purchases, int t) {
            energy -= cost;
            var level = ownedLevels[f.FactoryId] + 1;
            ownedLevels[f.FactoryId] = level;
            purchases.Add(new PurchaseEvent(t, f.FactoryId, level, cost));
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
        // Energy per second at each simulated second (always recorded, even without snapshots).
        public readonly IReadOnlyList<double> EnergyPerSecond;
        // Second at which each factory became unlocked (0 = unlocked from the start). Missing = never.
        public readonly IReadOnlyDictionary<string, int> UnlockTimes;
        public readonly IReadOnlyList<PurchaseEvent> Purchases;

        public int DurationSeconds => EnergyPerSecond?.Count ?? Snapshots?.Count ?? 0;

        public SimulationResult(IReadOnlyList<Snapshot> snapshots) {
            Snapshots = snapshots;
            EnergyPerSecond = snapshots.Select(s => s.EnergyPerSecond).ToList();
            UnlockTimes = new Dictionary<string, int>();
            Purchases = new List<PurchaseEvent>();
        }

        public SimulationResult(IReadOnlyList<Snapshot> snapshots, IReadOnlyList<double> energyPerSecond,
            IReadOnlyDictionary<string, int> unlockTimes, IReadOnlyList<PurchaseEvent> purchases) {
            Snapshots = snapshots;
            EnergyPerSecond = energyPerSecond;
            UnlockTimes = unlockTimes;
            Purchases = purchases;
        }
    }

    public readonly struct PurchaseEvent {
        public readonly int TimeSeconds;
        public readonly string FactoryId;
        public readonly int NewLevel;
        public readonly ulong Cost;

        public PurchaseEvent(int timeSeconds, string factoryId, int newLevel, ulong cost) {
            TimeSeconds = timeSeconds;
            FactoryId = factoryId;
            NewLevel = newLevel;
            Cost = cost;
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
