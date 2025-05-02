using System;
using System.Collections.Generic;
using System.Linq;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc.Economy.API {
    public class FactoryLevelsData : GoogleSpreadsheetScriptableObject, IFactoryLevelsData
    {
        [SerializeField] string factoryId;
        [SerializeField] string factoryName;
        [SerializeField] int startingLevel = 0;
        [SerializeField] Sprite icon;
        [SerializeField] FactoryType factoryType = FactoryType.Idle;

        [TableList]
        [SerializeField] List<LevelData> levelsList = new();

        public string FactoryId => factoryId;
        public int StartingLevel => startingLevel;

        public IReadOnlyList<LevelData> LevelsListReadOnly => levelsList;

        [ShowInInspector]
        [PreviewField(128)]
        public Sprite Icon => icon;
        public string FactoryName => factoryName;
        public FactoryType FactoryType => factoryType;

        protected override void ProcessData(GoogleSheetJson data)
        {
            levelsList.Clear();
            foreach (var sheetRow in data.values)
            {
                if (sheetRow.elements.Count <= 1)
                    continue;

                var columnIndex = 0;
                ParseIndexString(sheetRow.elements, columnIndex++, out var skip, "TRUE");
                if (skip == "TRUE")
                    continue;

                ParseIndexInt(sheetRow.elements, columnIndex++, out var level, 0);
                ParseIndexULong(sheetRow.elements, columnIndex++, out var cost, 0);
                ParseIndexULong(sheetRow.elements, columnIndex++, out var value, 0);
                ParseIndexInt(sheetRow.elements, columnIndex++, out var time, 1);

                levelsList.Add(new LevelData
                {
                    level = level,
                    cost = cost,
                    value = value,
                    time = time,
                });
                // if (sheetRow.elements.Count < 7) {
                //     Debug.LogError($"Missing element: {data.values.IndexOf(sheetRow)}");
                // }
            }
        }

        [System.Serializable]
        public class LevelData {
            [TableColumnWidth(100)] public int level;
            [TableColumnWidth(100)] public ulong cost;
            [TableColumnWidth(100)] public ulong value;
            [TableColumnWidth(100)] public int time;
        }

        public ulong GetCostForNextLevel(int factoryLevel) {
            var nextLevel = factoryLevel + 1;
            var levelData = levelsList.FirstOrDefault(x => x.level == nextLevel);
            if (levelData == null)
                return 0;
            return levelData.cost;
        }
        
        public ulong GetCostForLevel(int factoryLevel) {
            var nextLevel = factoryLevel;
            var levelData = levelsList.FirstOrDefault(x => x.level == nextLevel);
            if (levelData == null)
                return 0;
            return levelData.cost;
        }

        public ulong GetValueForLevel(int factoryLevel) {
            return GetValueForLevel(levelsList, factoryLevel);
        }

        public static ulong GetValueForLevel(List<LevelData> levels, int factoryLevel) {
            var levelData = levels.FirstOrDefault(x => x.level == factoryLevel);
            if (levelData != null)
                return levelData.value;

            var minLevel = levels.Min(x => x.level);
            var maxLevel = levels.Max(x => x.level);

            // If requested level is below the minimum or above the maximum, it's invalid
            if (factoryLevel <= minLevel || factoryLevel >= maxLevel)
                throw new InvalidOperationException($"Level {factoryLevel} is outside the valid range ({minLevel}-{maxLevel}).");

            var lower = levels.FirstOrDefault(x => x.level < factoryLevel);
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
        
        public int GetDurationForLevel(int factoryLevel) {
            var levelData = levelsList.FirstOrDefault(x => x.level == factoryLevel);
            if (levelData == null)
                return 0;
            return levelData.time;
        }

        public bool IsLastLevel(int factoryLevel) {
            return GetLastLevel() <= factoryLevel;
        }

        public int GetLastLevel() {
            return levelsList.Max(x => x.level);
        }

        public bool HasNextLevel(int factoryLevel) {
            var maxLevel = levelsList.Max(x => x.level);
            return maxLevel > factoryLevel;
        }
    }

    public interface IFactoryLevelsData {
        ulong GetCostForNextLevel(int factoryLevel);
        ulong GetCostForLevel(int factoryLevel);
        bool IsLastLevel(int factoryLevel);
        int GetLastLevel();
    }
}