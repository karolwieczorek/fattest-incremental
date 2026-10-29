using System.Collections.Generic;
using FattestInc.Progression.API;
using NUnit.Framework;

namespace FattestInc.Tests
{
    public class UnlockConditionsTests
    {
        Dictionary<string, int> levels;

        [SetUp]
        public void SetUp() {
            levels = new Dictionary<string, int>();
        }

        int GetLevel(string id) => levels.TryGetValue(id, out var level) ? level : 0;

        [Test]
        public void NullRule_IsMet() {
            Assert.IsTrue(UnlockConditions.AreMet(null, 0, 0, GetLevel));
        }

        [Test]
        public void EmptyRule_IsMet() {
            Assert.IsTrue(UnlockConditions.AreMet(new UnlockingFactoriesData.UnlockingData(), 0, 0, GetLevel));
        }

        [Test]
        [TestCase(99ul, false)]
        [TestCase(100ul, true)]
        public void Value_ChecksTotalEarned(ulong totalEarned, bool expected) {
            var rule = new UnlockingFactoriesData.UnlockingData { value = 100 };
            Assert.AreEqual(expected, UnlockConditions.AreMet(rule, totalEarned, 0, GetLevel));
        }

        [Test]
        [TestCase(9.9, false)]
        [TestCase(10.0, true)]
        public void ValuePerSecond_ChecksIncome(double amountPerSecond, bool expected) {
            var rule = new UnlockingFactoriesData.UnlockingData { valuePerSecond = 10 };
            Assert.AreEqual(expected, UnlockConditions.AreMet(rule, 0, amountPerSecond, GetLevel));
        }

        [Test]
        public void FactoryLevelZero_IsNoRequirement() {
            var rule = new UnlockingFactoriesData.UnlockingData { factory1Id = "missing", factory1Level = 0 };
            Assert.IsTrue(UnlockConditions.AreMet(rule, 0, 0, GetLevel));
        }

        [Test]
        public void AllFactoryRequirements_MustBeMet() {
            var rule = new UnlockingFactoriesData.UnlockingData {
                factory1Id = "a", factory1Level = 2,
                factory2Id = "b", factory2Level = 3,
                factory3Id = "c", factory3Level = 1,
            };
            levels["a"] = 2;
            levels["b"] = 3;
            Assert.IsFalse(UnlockConditions.AreMet(rule, 0, 0, GetLevel), "factory3 missing");

            levels["c"] = 1;
            Assert.IsTrue(UnlockConditions.AreMet(rule, 0, 0, GetLevel));

            levels["b"] = 2;
            Assert.IsFalse(UnlockConditions.AreMet(rule, 0, 0, GetLevel), "factory2 below level");
        }
    }
}
