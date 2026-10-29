using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using FattestInc.Simulation.API;
using NUnit.Framework;

namespace FattestInc.Tests
{
    public class SimulationReportTests
    {
        static FormulaFactory Clicker(string id, ulong output) => new() {
            factoryId = id, factoryName = id, factoryType = FactoryType.Clicker, startingLevel = 1,
            baseCost = 1, costMultiplier = 1, baseOutput = output, maxLevel = 1,
        };

        static FormulaFactory Idle(string id, ulong cost, ulong output, int maxLevel = 100) => new() {
            factoryId = id, factoryName = id, factoryType = FactoryType.Idle,
            baseCost = cost, costMultiplier = 1, baseOutput = output, maxLevel = maxLevel,
        };

        static Dictionary<string, UnlockingFactoriesData.UnlockingData> Rules(params UnlockingFactoriesData.UnlockingData[] rules) {
            var result = new Dictionary<string, UnlockingFactoriesData.UnlockingData>();
            foreach (var rule in rules)
                result[rule.factoryToUnlockId] = rule;
            return result;
        }

        [Test]
        public void RecordsUnlockAndPurchaseTimes() {
            var factories = new List<IFactoryData> { Clicker("click", 1), Idle("a", 10, 1, maxLevel: 1) };
            var rules = Rules(new UnlockingFactoriesData.UnlockingData { factoryToUnlockId = "a", unlockType = "Unlock", value = 5 });

            var result = new EconomySimulator().Run(factories, rules, new SimulationOptions { DurationSeconds = 20, ManualClicksPerSecond = 1 });

            Assert.AreEqual(0, result.UnlockTimes["click"]);
            // Earns 1/s: total earned reaches 5 at t=4, 10 energy at t=9.
            Assert.AreEqual(4, result.UnlockTimes["a"]);
            Assert.AreEqual(1, result.Purchases.Count);
            Assert.AreEqual(9, result.Purchases[0].TimeSeconds);
            Assert.AreEqual(20, result.DurationSeconds);
        }

        [Test]
        public void StopWhenUnlocked_EndsRunEarly() {
            var factories = new List<IFactoryData> { Clicker("click", 1), Idle("a", 10, 1) };
            var rules = Rules(new UnlockingFactoriesData.UnlockingData { factoryToUnlockId = "a", unlockType = "Unlock", valuePerSecond = 1 });

            var result = new EconomySimulator().Run(factories, rules,
                new SimulationOptions { DurationSeconds = 1000, ManualClicksPerSecond = 1, StopWhenUnlocked = "a" });

            Assert.AreEqual(0, result.UnlockTimes["a"]);
            Assert.AreEqual(1, result.DurationSeconds);
        }

        [Test]
        public void BestPayback_PrefersFasterPayback() {
            var factories = new List<IFactoryData> { Clicker("click", 1), Idle("good", 10, 5), Idle("bad", 10, 1) };

            var result = new EconomySimulator().Run(factories, null,
                new SimulationOptions { DurationSeconds = 30, ManualClicksPerSecond = 1, Strategy = PurchaseStrategy.BestPayback });

            Assert.AreEqual("good", result.Purchases[0].FactoryId);
            Assert.IsFalse(result.Purchases.Any(p => p.FactoryId == "bad"));
        }

        [Test]
        public void HighestTierFirst_BuysLastAffordable() {
            var factories = new List<IFactoryData> { Clicker("click", 1), Idle("good", 10, 5), Idle("bad", 10, 1) };

            var result = new EconomySimulator().Run(factories, null,
                new SimulationOptions { DurationSeconds = 30, ManualClicksPerSecond = 1, Strategy = PurchaseStrategy.HighestTierFirst });

            Assert.AreEqual("bad", result.Purchases[0].FactoryId);
        }

        [Test]
        public void Report_TiersAndGaps() {
            var factories = new List<IFactoryData> { Clicker("click", 1), Idle("a", 10, 1, maxLevel: 2) };
            var result = new EconomySimulator().Run(factories, null, new SimulationOptions { DurationSeconds = 60, ManualClicksPerSecond = 1 });
            var tiers = new List<(string, double)> { ("one", 1), ("two", 2), ("three", 3), ("never", 100) };

            var report = SimulationReport.Build(result, factories, tiers);

            // Purchases: level 1 at t=9 (10 energy), level 2 at t=14 (2/s from t=10, 10 more energy).
            Assert.AreEqual(9, report.Factories[1].FirstPurchaseTime);
            Assert.AreEqual(2, report.Factories[1].FinalLevel);
            Assert.AreEqual(0, report.Tiers[0].ReachedTime);
            Assert.AreEqual(10, report.Tiers[1].ReachedTime);
            Assert.AreEqual(15, report.Tiers[2].ReachedTime);
            Assert.AreEqual(SimulationReport.Never, report.Tiers[3].ReachedTime);

            var gap = report.LongestGap();
            Assert.AreEqual(0, gap.StartTime);
            Assert.AreEqual(9, gap.Length);
            Assert.AreEqual(5, report.LongestGap(9).Length);
            StringAssert.Contains("never", report.ToText());
        }

        [Test]
        [TestCase(1, 100ul)]
        [TestCase(2, 110ul)]
        [TestCase(3, 121ul)]
        public void FormulaFactory_CostGrowsByMultiplier(int level, ulong expected) {
            var factory = new FormulaFactory { baseCost = 100, costMultiplier = 1.1 };
            Assert.AreEqual(expected, factory.GetCostForLevel(level));
        }
    }
}
