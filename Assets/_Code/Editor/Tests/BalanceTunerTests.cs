using System.Collections.Generic;
using System.Linq;
using FattestInc.Balance.API;
using FattestInc.Economy.API;
using NUnit.Framework;

namespace FattestInc.Tests
{
    public class BalanceTunerTests
    {
        static List<FormulaFactory> Factories() {
            var list = new List<FormulaFactory> {
                new() { factoryId = "crank", factoryName = "crank", factoryType = FactoryType.Clicker, startingLevel = 1,
                    baseCost = 20, costMultiplier = 1.1, baseOutput = 2, maxLevel = 100 },
            };
            for (var i = 1; i <= 5; i++)
                list.Add(new FormulaFactory { factoryId = $"f{i}", factoryName = $"f{i}", maxLevel = 50 });
            return list;
        }

        static readonly string[] Tiers = { "t1", "t2", "t3", "t4" };

        static BalanceSettings Settings() => new() { firstUnlockSeconds = 20, lastUnlockSeconds = 600, lastTierSeconds = 900 };

        [Test]
        public void FactoriesUnlockAtTheirTargets() {
            var result = BalanceTuner.Tune(Factories(), Settings(), Tiers);

            for (var i = 1; i < result.Factories.Count; i++) {
                var unlock = result.Report.Factories[i].UnlockTime;
                // Gates are rounded down, so unlocks may come a little early but never late.
                Assert.LessOrEqual(unlock, result.TargetUnlockTimes[i], result.Factories[i].factoryId);
                Assert.GreaterOrEqual(unlock, result.TargetUnlockTimes[i] * 0.85, result.Factories[i].factoryId);
            }
            Assert.AreEqual(20, result.TargetUnlockTimes[1]);
            Assert.AreEqual(600, result.TargetUnlockTimes[^1]);
        }

        [Test]
        public void StarterIsUnchanged_OthersGetSettings() {
            var input = Factories();
            var result = BalanceTuner.Tune(input, Settings(), Tiers);

            Assert.AreEqual(input[0].baseCost, result.Factories[0].baseCost);
            Assert.AreEqual(input[0].costMultiplier, result.Factories[0].costMultiplier);
            Assert.IsTrue(result.Factories.Skip(1).All(f => f.costMultiplier == Settings().costMultiplier));
            Assert.IsTrue(result.Factories.Skip(2).Select((f, i) => f.baseCost >= result.Factories[i + 1].baseCost).All(x => x),
                "costs should not decrease from tier to tier");
        }

        [Test]
        public void UnlockRows_ShowBeforeUnlock() {
            var result = BalanceTuner.Tune(Factories(), Settings(), Tiers);

            foreach (var f in result.Factories.Skip(1)) {
                var show = result.UnlockRows.Single(r => r.factoryToUnlockId == f.factoryId && r.IsShowType);
                var unlock = result.UnlockRows.Single(r => r.factoryToUnlockId == f.factoryId && r.IsUnlockType);
                Assert.LessOrEqual(show.value, unlock.value, f.factoryId);
            }
            Assert.IsTrue(result.UnlockRows.Any(r => r.factoryToUnlockId == "crank" && r.IsUnlockType));
        }

        [Test]
        public void TierThresholds_StrictlyIncrease() {
            var result = BalanceTuner.Tune(Factories(), Settings(), Tiers);

            Assert.AreEqual(Tiers.Length, result.TierThresholds.Count);
            for (var i = 1; i < result.TierThresholds.Count; i++)
                Assert.Greater(result.TierThresholds[i], result.TierThresholds[i - 1]);
        }

        [Test]
        [TestCase(1234.0, 2, 1200ul)]
        [TestCase(99.9, 2, 99ul)]
        [TestCase(0.5, 2, 0ul)]
        [TestCase(56789.0, 1, 50000ul)]
        public void RoundDown(double value, int digits, ulong expected) {
            Assert.AreEqual(expected, BalanceTuner.RoundDown(value, digits));
        }

        [Test]
        [TestCase(1.0, 1ul)]
        [TestCase(4.9, 2ul)]
        [TestCase(5.0, 5ul)]
        [TestCase(999.0, 500ul)]
        [TestCase(2000.0, 2000ul)]
        public void NiceFloor(double value, ulong expected) {
            Assert.AreEqual(expected, BalanceTuner.NiceFloor(value));
        }
    }
}
