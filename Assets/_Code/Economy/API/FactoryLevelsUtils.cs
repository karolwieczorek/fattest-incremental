using System;
using System.Collections.Generic;
using System.Linq;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Economy.API {
    public static class FactoryLevelsUtils {

        public static ulong GetCostForLevel(List<FactoryLevelsData.LevelData> levels, int factoryLevel) {
            var levelData = levels.FirstOrDefault(x => x.level == factoryLevel);
            if (levelData != null)
                return levelData.cost;

            var minLevel = levels.Min(x => x.level);
            var maxLevel = levels.Max(x => x.level);

            // If requested level is below the minimum or above the maximum, it's invalid
            if (factoryLevel <= minLevel || factoryLevel >= maxLevel)
                throw new InvalidOperationException($"Level {factoryLevel} is outside the valid range ({minLevel}-{maxLevel}).");

            var lower = levels.LastOrDefaultEfficient(x => x.level < factoryLevel);
            var higher = levels.FirstOrDefault(x => x.level > factoryLevel);

            if (lower != null && higher != null) {
                var levelRange = higher.level - lower.level;
                var valueRange = higher.cost - lower.cost;
                var levelsBetween = factoryLevel - lower.level;

                var interpolatedValue = lower.cost + ((double)levelsBetween / levelRange) * valueRange;
                return (ulong)Math.Round(interpolatedValue);
            }

            // Missing either lower or higher for interpolation
            throw new InvalidOperationException($"Cannot interpolate level {factoryLevel}: missing lower or higher bounds.");
        }

        public static ulong GetValueForLevel(List<FactoryLevelsData.LevelData> levels, int factoryLevel) {
            var levelData = levels.FirstOrDefault(x => x.level == factoryLevel);
            if (levelData != null)
                return levelData.value;

            var minLevel = levels.Min(x => x.level);
            var maxLevel = levels.Max(x => x.level);

            // If requested level is below the minimum or above the maximum, it's invalid
            if (factoryLevel <= minLevel || factoryLevel >= maxLevel)
                throw new InvalidOperationException($"Level {factoryLevel} is outside the valid range ({minLevel}-{maxLevel}).");

            var lower = levels.LastOrDefaultEfficient(x => x.level < factoryLevel);
            var higher = levels.FirstOrDefault(x => x.level > factoryLevel);

            if (lower != null && higher != null)
            {
                var levelRange = higher.level - lower.level;
                var valueRange = higher.value - lower.value;
                var levelsBetween = factoryLevel - lower.level;

                var interpolatedValue = lower.value + ((double)levelsBetween / levelRange) * valueRange;
                return (ulong)Math.Round(interpolatedValue);
            }

            // Missing either lower or higher for interpolation
            throw new InvalidOperationException($"Cannot interpolate level {factoryLevel}: missing lower or higher bounds.");
        }

        public static int GetDurationForLevel(List<FactoryLevelsData.LevelData> levels, int factoryLevel) {
            var levelData = levels.FirstOrDefault(x => x.level == factoryLevel);
            if (levelData != null)
                return levelData.time;

            var minLevel = levels.Min(x => x.level);
            var maxLevel = levels.Max(x => x.level);

            // If requested level is below the minimum or above the maximum, it's invalid
            if (factoryLevel <= minLevel || factoryLevel >= maxLevel)
                throw new InvalidOperationException($"Level {factoryLevel} is outside the valid range ({minLevel}-{maxLevel}).");
            
            var lower = levels.LastOrDefaultEfficient(x => x.level < factoryLevel);
            var higher = levels.FirstOrDefault(x => x.level > factoryLevel);

            if (lower != null && higher != null)
                return Mathf.RoundToInt((lower.time + higher.time) / 2f);

            // Missing either lower or higher for interpolation
            throw new InvalidOperationException($"Cannot interpolate level {factoryLevel}: missing lower or higher bounds.");
        }
    }
}