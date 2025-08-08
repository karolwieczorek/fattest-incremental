using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;

namespace FattestInc.Simulation.API {
    public class EconomySimulator {
        public SimulationResult Run(IReadOnlyList<IFactoryData> factories,
            int durationSeconds,
            double manualClicksPerSecond,
            bool buyOnlyOnePerSecond = true,
            double startingEnergy = 0d) {
            if (factories == null || factories.Count == 0)
                return new SimulationResult(new List<Snapshot>());

            var ownedLevels = factories.ToDictionary(f => f.FactoryId, f => Math.Max(0, f.StartingLevel));
            double energy = startingEnergy;

            var snapshots = new List<Snapshot>(durationSeconds);

            for (int t = 0; t < durationSeconds; t++) {
                // Produce energy for this second
                energy += ProduceEnergyForSecond(factories, ownedLevels, manualClicksPerSecond);

                // Snapshot BEFORE purchasing (to match the provided pseudocode)
                var eps = CalculateEnergyPerSecond(factories, ownedLevels, manualClicksPerSecond);
                snapshots.Add(CreateSnapshot(t, energy, eps, factories, ownedLevels));

                // Purchase logic: iterate factories in reversed input order and buy first affordable
                TryPurchase(factories, ref energy, ownedLevels, buyOnlyOnePerSecond);
            }

            return new SimulationResult(snapshots);
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
            bool buyOnlyOnePerSecond) {
            if (energy <= 0)
                return;

            if (buyOnlyOnePerSecond) {
                for (int i = factories.Count - 1; i >= 0; i--) {
                    var f = factories[i];
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

        static Snapshot CreateSnapshot(int t, double energy, double eps,
            IReadOnlyList<IFactoryData> factories,
            Dictionary<string, int> ownedLevels) {
            var levels = new List<FactoryLevelSnapshot>(factories.Count);
            foreach (var f in factories) {
                levels.Add(new FactoryLevelSnapshot(f.FactoryId, f.FactoryName, ownedLevels[f.FactoryId]));
            }

            return new Snapshot(t, energy, eps, levels);
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
        public readonly double EnergyPerSecond;
        public readonly IReadOnlyList<FactoryLevelSnapshot> Levels;

        public Snapshot(int timeSeconds, double energy, double energyPerSecond, IReadOnlyList<FactoryLevelSnapshot> levels) {
            TimeSeconds = timeSeconds;
            Energy = energy;
            EnergyPerSecond = energyPerSecond;
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