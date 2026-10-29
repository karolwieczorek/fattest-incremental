using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using FattestInc.Simulation.API;

namespace FattestInc.Balance.API {
    // The few numbers the whole economy is generated from. Everything else (per-factory cost/output,
    // unlock gates, progression tier thresholds) is derived by BalanceTuner.
    [Serializable]
    public class BalanceSettings {
        // Player model used by the simulator.
        public double clicksPerSecond = 3d;

        // Pacing targets: when each new factory (after the starter) should unlock.
        public int firstUnlockSeconds = 20;
        public int lastUnlockSeconds = 2700;

        // A new factory's first level costs this many seconds of the player's income at its unlock time,
        // ramping from the first tuned factory to the last (short early so the first buildings come quickly).
        public double firstAffordSeconds = 15d;
        public double lastAffordSeconds = 60d;
        // A factory shows up (greyed out) this far between the previous unlock and its own (0 = with the previous one).
        public double showLeadFraction = 0.5d;

        // Per-level cost growth and level cap for every tuned factory (maxLevel 0 = keep each factory's own).
        public double costMultiplier = 1.15;
        public int maxLevel = 0;
        // Seconds for a factory's first level to pay for itself; grows per tier so later tiers feel weightier.
        public double firstPaybackSeconds = 15d;
        public double paybackGrowthPerTier = 1.25d;

        // Progression tiers (Phone ... Globe): when each should be reached.
        public int firstTierSeconds = 60;
        public int lastTierSeconds = 3300;

        // Keep generated costs/outputs readable.
        public int significantDigits = 2;
    }

    public class BalanceResult {
        public List<FormulaFactory> Factories;
        public List<UnlockingFactoriesData.UnlockingData> UnlockRows;
        public List<ulong> TierThresholds;
        public List<int> TargetUnlockTimes;
        public List<string> Warnings;
        public SimulationResult Simulation;
        public SimulationReport Report;
    }

    public static class BalanceTuner {
        const string UnlockType = "Unlock";
        const string ShowType = "Show";
        // Currency is ulong (max ~1.8e19). Keep the most expensive level well below that.
        const double MaxSafeCost = 1e18;

        // factories[0] is the starter and is kept as is. Each later factory n is priced from the simulated
        // economy of factories 0..n-1 at its target unlock time, and gated on the lifetime energy earned by then.
        public static BalanceResult Tune(IReadOnlyList<FormulaFactory> factories, BalanceSettings settings,
            IReadOnlyList<string> tierNames) {
            var tuned = new List<FormulaFactory> { Clone(factories[0]) };
            var gates = new List<(ulong show, ulong unlock)> { (0, 0) };
            var targets = new List<int> { 0 };
            var warnings = new List<string>();
            var tunedCount = factories.Count - 1;

            for (var n = 1; n < factories.Count; n++) {
                var target = GeometricTime(settings.firstUnlockSeconds, settings.lastUnlockSeconds, n - 1, tunedCount);
                var showTime = targets[^1] + (int)Math.Round((target - targets[^1]) * settings.showLeadFraction);
                // Up to its unlock, factory n doesn't exist for the player, so simulating 0..n-1 is exact.
                var simulation = Simulate(tuned, BuildUnlockRows(tuned, gates), target + 1, settings);
                var earned = TotalEarnedBySecond(simulation.EnergyPerSecond);
                var income = simulation.EnergyPerSecond[target];

                var payback = settings.firstPaybackSeconds * Math.Pow(settings.paybackGrowthPerTier, n - 1);
                var affordSeconds = GeometricValue(settings.firstAffordSeconds, settings.lastAffordSeconds, n - 1, tunedCount);
                var cost = Math.Max(1ul, RoundDown(income * affordSeconds, settings.significantDigits));
                var factory = Build(factories[n], cost, payback, settings);
                var maxCost = factory.baseCost * Math.Pow(factory.costMultiplier, factory.maxLevel - 1);
                if (maxCost > MaxSafeCost)
                    warnings.Add($"{factory.factoryId}: max level costs {maxCost:E1}, close to the ulong currency limit.");

                tuned.Add(factory);
                // Unlock exactly at the target (gate = earned by then, rounded down so it is reached on time).
                gates.Add((RoundDown(earned[showTime], settings.significantDigits), RoundDown(earned[target], settings.significantDigits)));
                targets.Add(target);
            }

            var rows = BuildUnlockRows(tuned, gates);
            var final = Simulate(tuned, rows, Math.Max(settings.lastTierSeconds, settings.lastUnlockSeconds) * 2, settings);
            var thresholds = BuildTierThresholds(final.IncomePerSecond, tierNames.Count, settings);
            var tiers = tierNames.Select((name, i) => (name, (double)thresholds[i])).ToList();

            return new BalanceResult {
                Factories = tuned,
                UnlockRows = rows,
                TierThresholds = thresholds,
                TargetUnlockTimes = targets,
                Warnings = warnings,
                Simulation = final,
                Report = SimulationReport.Build(final, tuned, tiers),
            };
        }

        public static SimulationResult Simulate(IReadOnlyList<FormulaFactory> factories,
            IReadOnlyList<UnlockingFactoriesData.UnlockingData> rows, int durationSeconds, BalanceSettings settings) {
            return new EconomySimulator().Run(factories.Cast<IFactoryData>().ToList(), ToRules(rows), new SimulationOptions {
                DurationSeconds = durationSeconds,
                ManualClicksPerSecond = settings.clicksPerSecond,
                BuyOnlyOnePerSecond = false,
                Strategy = PurchaseStrategy.BestPayback,
                RecordSnapshots = false,
            });
        }

        // Lifetime earned at the end of each second (production happens before unlock checks in the simulator).
        static List<double> TotalEarnedBySecond(IReadOnlyList<double> energyPerSecond) {
            var result = new List<double>(energyPerSecond.Count);
            var total = 0d;
            foreach (var eps in energyPerSecond) {
                total += eps;
                result.Add(total);
            }

            return result;
        }

        static FormulaFactory Build(FormulaFactory template, ulong cost, double payback, BalanceSettings settings) {
            var factory = Clone(template);
            factory.baseCost = Math.Max(1ul, cost);
            factory.costMultiplier = settings.costMultiplier;
            factory.linearOutputByLevel = true;
            if (settings.maxLevel > 0)
                factory.maxLevel = settings.maxLevel;
            // Level 1 earns its cost back in `payback` seconds (clickers at the modelled click rate).
            var perSecond = factory.baseCost / payback;
            var perTick = factory.factoryType == FactoryType.Clicker
                ? perSecond / Math.Max(settings.clicksPerSecond, 0.01d)
                : perSecond * Math.Max(1, factory.durationSeconds);
            factory.baseOutput = Math.Max(1ul, RoundDown(perTick, settings.significantDigits));
            return factory;
        }

        static List<UnlockingFactoriesData.UnlockingData> BuildUnlockRows(IReadOnlyList<FormulaFactory> factories,
            IReadOnlyList<(ulong show, ulong unlock)> gates) {
            var rows = new List<UnlockingFactoriesData.UnlockingData>();
            var index = 1;
            for (var n = 0; n < factories.Count; n++) {
                var id = factories[n].factoryId;
                if (n == 0) {
                    rows.Add(new UnlockingFactoriesData.UnlockingData { index = index++, factoryToUnlockId = id, unlockType = UnlockType });
                    continue;
                }

                rows.Add(new UnlockingFactoriesData.UnlockingData { index = index++, factoryToUnlockId = id, unlockType = ShowType, value = gates[n].show });
                rows.Add(new UnlockingFactoriesData.UnlockingData { index = index++, factoryToUnlockId = id, unlockType = UnlockType, value = gates[n].unlock });
            }

            return rows;
        }

        static List<ulong> BuildTierThresholds(IReadOnlyList<double> income, int tierCount, BalanceSettings settings) {
            var thresholds = new List<ulong>();
            for (var k = 0; k < tierCount; k++) {
                var time = GeometricTime(settings.firstTierSeconds, settings.lastTierSeconds, k, tierCount);
                var value = income.Count == 0 ? 1d : income[Math.Min(time, income.Count - 1)];
                var threshold = NiceFloor(Math.Max(1d, value));
                // Keep tiers strictly increasing even where income stalls.
                if (thresholds.Count > 0 && threshold <= thresholds[^1])
                    threshold = NiceAbove(thresholds[^1]);
                thresholds.Add(threshold);
            }

            return thresholds;
        }

        static Dictionary<string, UnlockingFactoriesData.UnlockingData> ToRules(IReadOnlyList<UnlockingFactoriesData.UnlockingData> rows) {
            var rules = new Dictionary<string, UnlockingFactoriesData.UnlockingData>();
            foreach (var row in rows) {
                if (row.IsUnlockType)
                    rules.TryAdd(row.factoryToUnlockId, row);
            }

            return rules;
        }

        // Time of point i out of `count` points on a geometric curve from first to last.
        public static int GeometricTime(int first, int last, int i, int count) {
            return (int)Math.Round(GeometricValue(first, last, i, count));
        }

        public static double GeometricValue(double first, double last, int i, int count) {
            if (count <= 1)
                return first;
            var ratio = Math.Pow(last / first, 1d / (count - 1));
            return first * Math.Pow(ratio, i);
        }

        public static ulong RoundDown(double value, int significantDigits) {
            if (value < 1d)
                return 0;
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)) - (significantDigits - 1));
            return (ulong)(Math.Floor(value / magnitude * (1 + 1e-12)) * magnitude);
        }

        // Largest 1/2/5 * 10^k not above value.
        public static ulong NiceFloor(double value) {
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            var mantissa = value / magnitude * (1 + 1e-12);
            var nice = mantissa >= 5 ? 5 : mantissa >= 2 ? 2 : 1;
            return (ulong)(nice * magnitude);
        }

        static ulong NiceAbove(ulong value) {
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            var mantissa = value / magnitude;
            var next = mantissa < 2 ? 2 : mantissa < 5 ? 5 : 10;
            return (ulong)(next * magnitude);
        }

        static FormulaFactory Clone(FormulaFactory f) {
            return new FormulaFactory {
                factoryId = f.factoryId,
                factoryName = f.factoryName,
                factoryType = f.factoryType,
                startingLevel = f.startingLevel,
                maxLevel = f.maxLevel,
                baseCost = f.baseCost,
                costMultiplier = f.costMultiplier,
                baseOutput = f.baseOutput,
                linearOutputByLevel = f.linearOutputByLevel,
                durationSeconds = f.durationSeconds,
            };
        }
    }
}
