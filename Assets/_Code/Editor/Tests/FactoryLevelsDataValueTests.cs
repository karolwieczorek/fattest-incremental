using System;
using System.Collections.Generic;
using FattestInc.Economy.API;
using NUnit.Framework;

namespace FattestInc.Tests {
    public class FactoryLevelsDataValueTests {
        [Test]
        public void ReturnsExactMatch_WhenLevelExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, value = 100 },
                new() { level = 2, value = 200 }
            };

            var result = FactoryLevelsUtils.GetValueForLevel(levels, 2);

            Assert.AreEqual((ulong)200, result);
        }

        [Test]
        public void InterpolatesCorrectly_BetweenTwoLevels() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, value = 5 },
                new () { level = 6, value = 15 }
            };

            var result = FactoryLevelsUtils.GetValueForLevel(levels, 3);
            const ulong expectedValue = 9;
            
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ThrowsException_WhenBelowMinimumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, value = 100 },
                new () { level = 5, value = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetValueForLevel(levels, 0));
        }

        [Test]
        public void ThrowsException_WhenAboveMaximumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, value = 100 },
                new () { level = 5, value = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetValueForLevel(levels, 6));
        }

        [Test]
        public void ThrowsException_WhenNoLowerBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData>
            {
                new () { level = 5, value = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetValueForLevel(levels, 3));
        }

        [Test]
        public void ThrowsException_WhenNoHigherBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, value = 100 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetValueForLevel(levels, 3));
        }
    }
}