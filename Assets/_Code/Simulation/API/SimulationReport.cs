using System;
using System.Collections.Generic;
using System.Text;
using FattestInc.Economy.API;

namespace FattestInc.Simulation.API {
    // Pacing numbers derived from a simulation run: when things unlock and how long the player waits.
    public class SimulationReport {
        public const int Never = -1;

        public readonly struct FactoryTiming {
            public readonly string FactoryId;
            public readonly string FactoryName;
            public readonly int UnlockTime;
            public readonly int FirstPurchaseTime;
            public readonly int FinalLevel;

            public FactoryTiming(string factoryId, string factoryName, int unlockTime, int firstPurchaseTime, int finalLevel) {
                FactoryId = factoryId;
                FactoryName = factoryName;
                UnlockTime = unlockTime;
                FirstPurchaseTime = firstPurchaseTime;
                FinalLevel = finalLevel;
            }
        }

        public readonly struct TierTiming {
            public readonly string Name;
            public readonly double Threshold;
            public readonly int ReachedTime;

            public TierTiming(string name, double threshold, int reachedTime) {
                Name = name;
                Threshold = threshold;
                ReachedTime = reachedTime;
            }
        }

        public readonly struct Gap {
            public readonly int StartTime;
            public readonly int Length;

            public Gap(int startTime, int length) {
                StartTime = startTime;
                Length = length;
            }
        }

        public IReadOnlyList<FactoryTiming> Factories { get; }
        public IReadOnlyList<TierTiming> Tiers { get; }
        public int DurationSeconds { get; }
        public int PurchaseCount { get; }

        readonly List<int> purchaseTimes;

        SimulationReport(List<FactoryTiming> factories, List<TierTiming> tiers, List<int> purchaseTimes, int durationSeconds) {
            Factories = factories;
            Tiers = tiers;
            this.purchaseTimes = purchaseTimes;
            DurationSeconds = durationSeconds;
            PurchaseCount = purchaseTimes.Count;
        }

        public static SimulationReport Build(SimulationResult result, IReadOnlyList<IFactoryData> factories,
            IReadOnlyList<(string name, double threshold)> tiers = null) {
            var firstPurchase = new Dictionary<string, int>();
            var finalLevels = new Dictionary<string, int>();
            var purchaseTimes = new List<int>();
            foreach (var purchase in result.Purchases) {
                if (firstPurchase.ContainsKey(purchase.FactoryId) == false)
                    firstPurchase[purchase.FactoryId] = purchase.TimeSeconds;
                finalLevels[purchase.FactoryId] = purchase.NewLevel;
                // Several purchases in the same second count as one moment for the player.
                if (purchaseTimes.Count == 0 || purchaseTimes[^1] != purchase.TimeSeconds)
                    purchaseTimes.Add(purchase.TimeSeconds);
            }

            var factoryTimings = new List<FactoryTiming>();
            foreach (var f in factories) {
                var unlock = result.UnlockTimes.TryGetValue(f.FactoryId, out var u) ? u : Never;
                var first = firstPurchase.TryGetValue(f.FactoryId, out var p) ? p : Never;
                var level = finalLevels.TryGetValue(f.FactoryId, out var l) ? l : Math.Max(0, f.StartingLevel);
                factoryTimings.Add(new FactoryTiming(f.FactoryId, f.FactoryName, unlock, first, level));
            }

            var tierTimings = new List<TierTiming>();
            if (tiers != null) {
                foreach (var (name, threshold) in tiers)
                    tierTimings.Add(new TierTiming(name, threshold, FindFirstSecondReaching(result.EnergyPerSecond, threshold)));
            }

            return new SimulationReport(factoryTimings, tierTimings, purchaseTimes, result.DurationSeconds);
        }

        public static int FindFirstSecondReaching(IReadOnlyList<double> energyPerSecond, double threshold) {
            if (energyPerSecond == null)
                return Never;
            for (var t = 0; t < energyPerSecond.Count; t++) {
                if (energyPerSecond[t] >= threshold)
                    return t;
            }

            return Never;
        }

        // Longest wait between two purchases that starts inside [fromSecond, toSecond).
        // The wait before the first purchase counts too (starting at 0).
        public Gap LongestGap(int fromSecond = 0, int toSecond = int.MaxValue) {
            var best = new Gap(0, 0);
            var previous = 0;
            foreach (var time in purchaseTimes) {
                if (previous >= fromSecond && previous < toSecond && time - previous > best.Length)
                    best = new Gap(previous, time - previous);
                previous = time;
            }

            return best;
        }

        public string ToText() {
            var sb = new StringBuilder();
            sb.AppendLine($"Simulated {FormatTime(DurationSeconds)}, {PurchaseCount} purchase moments");
            sb.AppendLine();
            sb.AppendLine("Factory              unlock     first buy  final lvl");
            foreach (var f in Factories)
                sb.AppendLine($"{f.FactoryName,-20} {FormatTime(f.UnlockTime),-10} {FormatTime(f.FirstPurchaseTime),-10} {f.FinalLevel}");

            if (Tiers.Count > 0) {
                sb.AppendLine();
                sb.AppendLine("Tier                 needs/s        reached");
                foreach (var tier in Tiers)
                    sb.AppendLine($"{tier.Name,-20} {tier.Threshold,-14:N0} {FormatTime(tier.ReachedTime)}");
            }

            sb.AppendLine();
            sb.AppendLine("Longest wait between purchases");
            AppendGap(sb, "0-5 min", LongestGap(0, 300));
            AppendGap(sb, "5-30 min", LongestGap(300, 1800));
            AppendGap(sb, "30 min+", LongestGap(1800));
            return sb.ToString();
        }

        static void AppendGap(StringBuilder sb, string label, Gap gap) {
            sb.AppendLine($"  {label,-10} {FormatTime(gap.Length)} (from {FormatTime(gap.StartTime)})");
        }

        public static string FormatTime(int seconds) {
            if (seconds == Never)
                return "never";
            var span = TimeSpan.FromSeconds(seconds);
            return span.TotalHours >= 1 ? $"{(int)span.TotalHours}h{span.Minutes:00}m" : $"{span.Minutes}m{span.Seconds:00}s";
        }
    }
}
