﻿using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class FactoryLevelsData : GoogleSpreadsheetScriptableObject
    {
        [SerializeField] string factoryId;

        [TableList]
        [SerializeField] List<LevelData> levelsList = new();

        public string FactoryId => factoryId;

        public IReadOnlyList<LevelData> LevelsListReadOnly => levelsList;

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
                ParseIndexInt(sheetRow.elements, columnIndex++, out var cost, 0);
                ParseIndexInt(sheetRow.elements, columnIndex++, out var value, 0);
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

        void ParseIndexString(List<string> elements, int index, out string value, string defaultValue) {
            if (elements.Count >= index + 1) {
                value = elements[index];
                return;
            }

            value = defaultValue;
        }
        
        void ParseIndexInt(List<string> elements, int index, out int value, int defaultValue = 0) {
            if (elements.Count >= index + 1 && int.TryParse(elements[index], out var parsedValue)) {
                value = parsedValue;
                return;
            }

            value = defaultValue;
        }
        
        void ParseIndexFloat(List<string> elements, int index, out float value, float defaultValue = 0) {
            if (elements.Count >= index + 1 && float.TryParse(elements[index], out var parsedValue)) {
                value = parsedValue;
                return;
            }

            value = defaultValue;
        }

        [System.Serializable]
        public class LevelData {
            [TableColumnWidth(100)] public int level;
            [TableColumnWidth(100)] public int cost;
            [TableColumnWidth(100)] public int value;
            [TableColumnWidth(100)] public int time;
        }
    }
}