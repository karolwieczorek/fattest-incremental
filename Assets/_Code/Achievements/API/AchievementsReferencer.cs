using System.Collections.Generic;
using Hypnagogia.Utils;
using UnityEngine;

namespace FattestInc.Achievements.API {
    public class AchievementsReferencer : HReferencer {
        [SerializeField] List<AchievementDefinition> achievements = new();
        [Tooltip("Steam-style notification shown when an achievement unlocks. Has its own overlay canvas.")]
        [SerializeField] GameObject toastPrefab;

        public IReadOnlyList<AchievementDefinition> Achievements => achievements;
        public GameObject ToastPrefab => toastPrefab;

        public AchievementDefinition Find(string id) => achievements.Find(a => a.id == id);
    }
}
