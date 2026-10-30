using FattestInc.Achievements.API;
using Hypnagogia.Utils;
using UnityEngine;
using Zenject;

namespace FattestInc.Achievements.Implementation {
    // Shows the toast for each newly unlocked achievement. The toast prefab has its own overlay canvas,
    // so it is spawned on first use instead of living in the scene.
    public class AchievementToastSystem : HSystem {
        [HInject] AchievementsDataStore achievementsDataStore;
        [HInject] AchievementsReferencer achievementsReferencer;
        [HInject] DiContainer container;

        AchievementToastView toastView;

        protected override void SystemStart() {
            achievementsDataStore.AchievementUnlocked += OnAchievementUnlocked;
        }

        protected override void SystemStop() {
            achievementsDataStore.AchievementUnlocked -= OnAchievementUnlocked;
        }

        void OnAchievementUnlocked(string id) {
            var achievement = achievementsReferencer.Find(id);
            if (achievement == null)
                return;
            if (achievementsReferencer.ToastPrefab == null) {
                Debug.LogWarning("No achievement toast prefab set in AchievementsReferencer.");
                return;
            }

            if (toastView == null)
                toastView = container.InstantiatePrefabForComponent<AchievementToastView>(achievementsReferencer.ToastPrefab);
            toastView.Enqueue(achievement);
        }
    }
}
