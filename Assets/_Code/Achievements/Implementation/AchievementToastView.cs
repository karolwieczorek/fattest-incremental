using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FattestInc.Achievements.API;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Achievements.Implementation {
    // Steam-style notification: slides in from the right edge, stays a few seconds, slides out.
    // Several unlocks at once are shown one after another.
    public class AchievementToastView : MonoBehaviour {
        [SerializeField] RectTransform panel;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text descriptionLabel;
        [SerializeField] float slideDuration = 0.35f;
        [SerializeField] float visibleDuration = 4f;

        readonly Queue<AchievementDefinition> queue = new();
        Vector2 shownPosition;
        bool isShowing;

        void Awake() {
            shownPosition = panel.anchoredPosition;
            canvasGroup.alpha = 0f;
        }

        public void Enqueue(AchievementDefinition achievement) {
            queue.Enqueue(achievement);
            if (!isShowing)
                ShowQueue(destroyCancellationToken).Forget();
        }

        async UniTaskVoid ShowQueue(CancellationToken cancellationToken) {
            isShowing = true;
            while (queue.Count > 0) {
                var achievement = queue.Dequeue();
                titleLabel.text = achievement.title;
                descriptionLabel.text = achievement.description;
                icon.sprite = achievement.icon;
                icon.enabled = achievement.icon != null;

                var hiddenPosition = shownPosition + new Vector2(panel.rect.width + 100f, 0f);
                await Slide(hiddenPosition, shownPosition, 0f, 1f, Ease.OutCubic, cancellationToken);
                await UniTask.WaitForSeconds(visibleDuration, cancellationToken: cancellationToken);
                await Slide(shownPosition, hiddenPosition, 1f, 0f, Ease.InCubic, cancellationToken);
            }

            isShowing = false;
        }

        async UniTask Slide(Vector2 from, Vector2 to, float fromAlpha, float toAlpha, Ease ease, CancellationToken cancellationToken) {
            var handle = LMotion.Create(0f, 1f, slideDuration).WithEase(ease).Bind(t => {
                panel.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            });
            while (handle.IsActive())
                await UniTask.Yield(cancellationToken);
        }
    }
}
