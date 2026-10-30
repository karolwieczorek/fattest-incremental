using System.Collections.Generic;
using FattestInc.Achievements.API;
using FattestInc.Economy.API;
using NUnit.Framework;

namespace FattestInc.Tests
{
    public class AchievementConditionsTests
    {
        static readonly List<IFactoryData> Factories = new() {
            new FormulaFactory { factoryId = "a", maxLevel = 3 },
            new FormulaFactory { factoryId = "b", maxLevel = 5 },
        };

        static bool IsMet(AchievementCondition condition, Dictionary<string, int> levels, double income = 0, ulong lastTier = 1000) {
            return AchievementConditions.IsMet(condition, Factories,
                id => levels.TryGetValue(id, out var level) ? level : 0, income, lastTier);
        }

        [Test]
        public void MaxAllFactories_NeedsEveryFactoryAtMax() {
            Assert.IsFalse(IsMet(AchievementCondition.MaxAllFactories, new Dictionary<string, int> { ["a"] = 3, ["b"] = 4 }));
            Assert.IsFalse(IsMet(AchievementCondition.MaxAllFactories, new Dictionary<string, int> { ["a"] = 3 }));
            Assert.IsTrue(IsMet(AchievementCondition.MaxAllFactories, new Dictionary<string, int> { ["a"] = 3, ["b"] = 5 }));
        }

        [Test]
        public void MaxAllFactories_NoFactories_IsNotMet() {
            Assert.IsFalse(AchievementConditions.AreAllFactoriesMaxed(new List<IFactoryData>(), _ => 0));
        }

        [Test]
        [TestCase(999.0, false)]
        [TestCase(1000.0, true)]
        public void ReachLastProgressionTier_ComparesIncome(double income, bool expected) {
            Assert.AreEqual(expected, IsMet(AchievementCondition.ReachLastProgressionTier, new Dictionary<string, int>(), income));
        }

        [Test]
        public void ReachLastProgressionTier_NoTiers_IsNotMet() {
            Assert.IsFalse(IsMet(AchievementCondition.ReachLastProgressionTier, new Dictionary<string, int>(), 1e9, lastTier: 0));
        }
    }
}
