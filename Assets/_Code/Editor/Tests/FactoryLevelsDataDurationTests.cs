using System;
using System.Collections.Generic;
using FattestInc.Economy.API;
using NUnit.Framework;

namespace FattestInc.Tests {
    public class FactoryLevelsDataDurationTests {
        [Test]
        public void ReturnsExactMatch_WhenLevelExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, time = 100 },
                new() { level = 2, time = 200 }
            };

            var result = FactoryLevelsUtils.GetDurationForLevel(levels, 2);

            Assert.AreEqual((ulong)200, result);
        }

        [Test]
        public void InterpolatesCorrectly_BetweenTwoLevels() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, time = 10 },
                new () { level = 6, time = 20 }
            };

            var result = FactoryLevelsUtils.GetDurationForLevel(levels, 3);
            const ulong expectedValue = 15;
            
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ThrowsException_WhenBelowMinimumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, time = 100 },
                new () { level = 5, time = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetDurationForLevel(levels, 0));
        }

        [Test]
        public void ThrowsException_WhenAboveMaximumLevel() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new () { level = 1, time = 100 },
                new () { level = 5, time = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetDurationForLevel(levels, 6));
        }

        [Test]
        public void ThrowsException_WhenNoLowerBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData>
            {
                new () { level = 5, time = 500 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetDurationForLevel(levels, 3));
        }

        [Test]
        public void ThrowsException_WhenNoHigherBoundExists() {
            var levels = new List<FactoryLevelsData.LevelData> {
                new() { level = 1, time = 100 }
            };

            Assert.Throws<InvalidOperationException>(() => FactoryLevelsUtils.GetDurationForLevel(levels, 3));
        }
    }
}