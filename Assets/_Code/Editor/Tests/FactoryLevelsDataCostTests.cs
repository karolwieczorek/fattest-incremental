using System;
using System.Collections.Generic;
using FattestInc.Economy.API;
using NUnit.Framework;

namespace FattestInc.Tests {
    public class FactoryLevelsDataCostTests {
        [Test]
        public void ReturnsExactMatch_WhenLevelExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, cost = 100 },
                new() { level = 2, cost = 200 }
            };

            var result = FactoryLevelsUtils.GetCostForLevel(levels, 2);

            Assert.AreEqual((ulong)200, result);
        }

        [Test]
        public void InterpolatesCorrectly_BetweenTwoLevels() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, cost = 5 },
                new () { level = 6, cost = 15 }
            };

            var result = FactoryLevelsUtils.GetCostForLevel(levels, 3);
            const ulong expectedValue = 9;
            
            Assert.AreEqual(expectedValue, result);
        }
        
        [Test]
        public void InterpolatesCorrectly_BetweenTwoLevels_HandCrank() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, cost = 10 },
                new () { level = 2, cost = 40 },
                new () { level = 3, cost = 40 },
                new () { level = 4, cost = 40 },
                new () { level = 40, cost = 40 },
                new () { level = 400, cost = 400000 },
            };

            var result = FactoryLevelsUtils.GetCostForLevel(levels, 30);
            const ulong expectedValue = 40;
            
            Assert.AreEqual(expectedValue, result);
        }
        
        [Test]
        [TestCase(5, 2, 40ul, 40ul, 40ul)]
        [TestCase(3, 2, 5ul, 20ul, 15ul)]
        public void InterpolatesCorrectly_BetweenTwoLevels2(int levelsCount, int levelsIncrement, ulong constMin, ulong costMax, ulong costExpected) {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, cost = constMin },
                new () { level = 1+levelsCount, cost = costMax }
            };

            var result = FactoryLevelsUtils.GetCostForLevel(levels, 1 + levelsIncrement);
            ulong expectedValue = costExpected;
            
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ThrowsException_WhenBelowMinimumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, cost = 100 },
                new () { level = 5, cost = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetCostForLevel(levels, 0));
        }

        [Test]
        public void ThrowsException_WhenAboveMaximumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, cost = 100 },
                new () { level = 5, cost = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetCostForLevel(levels, 6));
        }

        [Test]
        public void ThrowsException_WhenNoLowerBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData>
            {
                new () { level = 5, cost = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetCostForLevel(levels, 3));
        }

        [Test]
        public void ThrowsException_WhenNoHigherBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, cost = 100 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetCostForLevel(levels, 3));
        }
    }
}