using System;
using System.Collections.Generic;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;

namespace FattestInc.Achievements.API {
    public class AchievementsDataStore : HDataStore {
        // Achievement id -> unlock time (unix seconds, UTC).
        [ShowInInspector] readonly Dictionary<string, long> unlocked = new();

        public IReadOnlyDictionary<string, long> Unlocked => unlocked;
        public event Action<string> AchievementUnlocked;

        public bool IsUnlocked(string id) => unlocked.ContainsKey(id);

        public bool TryUnlock(string id) {
            if (string.IsNullOrEmpty(id) || unlocked.ContainsKey(id))
                return false;
            unlocked[id] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            AchievementUnlocked?.Invoke(id);
            return true;
        }

        // Loading never raises AchievementUnlocked.
        public void Load(string id, long unlockTime) {
            if (!string.IsNullOrEmpty(id))
                unlocked[id] = unlockTime;
        }

        public void Clear() {
            unlocked.Clear();
        }
    }
}
