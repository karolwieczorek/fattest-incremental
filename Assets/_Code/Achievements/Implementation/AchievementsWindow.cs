using System.Collections.Generic;
using FattestInc.Achievements.API;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;

namespace FattestInc.Achievements.Implementation {
    public class AchievementsWindow : SimpleWindow {
        [SerializeField] Transform listParent;
        [Tooltip("Inactive row used as a template for each achievement.")]
        [SerializeField] AchievementEntryView entryTemplate;
        [SerializeField] TMP_Text summaryLabel;

        [HInject] AchievementsReferencer achievementsReferencer;
        [HInject] AchievementsDataStore achievementsDataStore;

        readonly List<AchievementEntryView> entries = new();

        protected override void BeforeShowInitialize() {
            var achievements = achievementsReferencer.Achievements;
            while (entries.Count < achievements.Count) {
                var entry = Instantiate(entryTemplate, listParent);
                entry.gameObject.SetActive(true);
                entries.Add(entry);
            }

            var unlockedCount = 0;
            for (var i = 0; i < achievements.Count; i++) {
                var achievement = achievements[i];
                long? unlockTime = achievementsDataStore.Unlocked.TryGetValue(achievement.id, out var time) ? time : null;
                if (unlockTime.HasValue)
                    unlockedCount++;
                entries[i].Show(achievement, unlockTime);
            }

            if (summaryLabel != null)
                summaryLabel.text = $"{unlockedCount} / {achievements.Count}";
        }
    }
}
