using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class UnlockingFactoriesData : GoogleSpreadsheetScriptableObject {
        [TableList]
        [SerializeField] List<UnlockingData> levelsList = new();

        public List<UnlockingData> LevelsList => levelsList;

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
                ParseIndexString(sheetRow.elements, columnIndex++, out var factoryToUnlockId, "None");
                ParseIndexString(sheetRow.elements, columnIndex++, out var unlockType, "None");
                ParseIndexULong(sheetRow.elements, columnIndex++, out var value, 0);
                ParseIndexInt(sheetRow.elements, columnIndex++, out var valuePerSecond, 0);
                ParseIndexString(sheetRow.elements, columnIndex++, out var factory1Id, "");
                ParseIndexInt(sheetRow.elements, columnIndex++, out var factory1Level, 0);

                LevelsList.Add(new UnlockingData
                {
                    index = index,
                    factoryToUnlockId = factoryToUnlockId,
                    unlockType = unlockType,
                    value = value,
                    valuePerSecond = valuePerSecond,
                    factory1Id = factory1Id,
                    factory1Level = factory1Level,
                    
                });
            }
        }

        [System.Serializable]
        public class UnlockingData {
            [TableColumnWidth(30)] public int index;
            [TableColumnWidth(100)] public string factoryToUnlockId;
            [TableColumnWidth(60)] public string unlockType;
            [TableColumnWidth(100)] public ulong value;
            [TableColumnWidth(100)] public int valuePerSecond;
            [TableColumnWidth(100)] public string factory1Id;
            [TableColumnWidth(30)] public int factory1Level;
            [TableColumnWidth(100)] public string factory2Id;
            [TableColumnWidth(30)] public int factory2Level;
            [TableColumnWidth(100)] public string factory3Id;
            [TableColumnWidth(30)] public int factory3Level;

            public bool IsShowType => unlockType == "Show";
            public bool IsUnlockType => unlockType == "Unlock";
        }
    }
}