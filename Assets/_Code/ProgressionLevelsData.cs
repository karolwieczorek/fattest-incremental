using System.Collections.Generic;
using System.Linq;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class ProgressionLevelsData : GoogleSpreadsheetScriptableObject {
        [TableList]
        [SerializeField] List<LevelData> levelsList = new();

        [SerializeField] List<IconNamePair> icons;

        public List<LevelData> LevelsList => levelsList;

        protected override void ProcessData(GoogleSheetJson data)
        {
            LevelsList.Clear();
            foreach (var sheetRow in data.values)
            {
                if (sheetRow.elements.Count <= 1)
                    continue;

                var columnIndex = 0;
                ParseIndexString(sheetRow.elements, columnIndex++, out var skip, "TRUE");
                if (skip == "TRUE")
                    continue;

                ParseIndexInt(sheetRow.elements, columnIndex++, out var index, 0);
                ParseIndexString(sheetRow.elements, columnIndex++, out var name, "None");
                ParseIndexULong(sheetRow.elements, columnIndex++, out var value, 0);

                LevelsList.Add(new LevelData
                {
                    index = index,
                    name = name,
                    value = value,
                    sprite = icons.FirstOrDefault(pair => pair.name == name)?.icon
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
        
        void ParseIndexULong(List<string> elements, int index, out ulong value, ulong defaultValue = 0) {
            if (elements.Count >= index + 1 && ulong.TryParse(elements[index], out var parsedValue)) {
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
            [TableColumnWidth(100)] public int index;
            [TableColumnWidth(100)] public string name;
            [TableColumnWidth(100)] public ulong value;
            [TableColumnWidth(100)] public Sprite sprite;
        }

        [System.Serializable]
        public class IconNamePair {
            public string name;
            public Sprite icon;
        }
    }
}