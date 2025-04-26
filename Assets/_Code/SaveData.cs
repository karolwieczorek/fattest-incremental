using System;
using System.Collections.Generic;

namespace FattestInc {
    [Serializable]
    public class SaveData {
        public ulong currencyAmount;
        public List<FactoryLevelEntry> factoryLevels = new();

        [Serializable]
        public class FactoryLevelEntry {
            public string factoryId;
            public int level;
        }

        public void SetFactoryLevel(string id, int level) {
            var entry = factoryLevels.Find(e => e.factoryId == id);
            if (entry != null)
                entry.level = level;
            else
                factoryLevels.Add(new FactoryLevelEntry {factoryId = id, level = level});
        }

        public int GetFactoryLevel(string id) {
            var entry = factoryLevels.Find(e => e.factoryId == id);
            return entry?.level ?? 0;
        }
    }
}