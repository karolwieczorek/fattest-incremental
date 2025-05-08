using System.Collections.Generic;

namespace Hypnagogia.Utils {
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> {
        public List<TKey> keys = new();
        public List<TValue> values = new();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<TKey, TValue> dict) {
            foreach (var kvp in dict) {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary() {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++) {
                dict[keys[i]] = values[i];
            }

            return dict;
        }
    }
}