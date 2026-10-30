using System;
using FattestInc.Achievements.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Achievements.Implementation {
    public class AchievementEntryView : MonoBehaviour {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text descriptionLabel;
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float lockedAlpha = 0.45f;

        public void Show(AchievementDefinition achievement, long? unlockTime) {
            icon.sprite = achievement.icon;
            icon.enabled = achievement.icon != null;
            titleLabel.text = achievement.title;
            descriptionLabel.text = achievement.description;
            statusLabel.text = unlockTime.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(unlockTime.Value).ToLocalTime().ToString("yyyy-MM-dd")
                : "Locked";
            canvasGroup.alpha = unlockTime.HasValue ? 1f : lockedAlpha;
        }
    }
}
