using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc {
    public class ProgressionLevelView : MonoBehaviour {
        [SerializeField] Image currentBackground;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] GameObject progressBarContent;
        [SerializeField] Image progressBarFillImage;
        [SerializeField] TMP_Text progressBarAmountLabel;
        
        [SerializeField] float futureAlpha = 0.5f;
        [SerializeField] float standardHeight = 110;
        [SerializeField] float currentHeight = 150;

        ProgressionLevelsData.LevelData levelData;

        public ulong Value => levelData?.value ?? 0;

        public void Init(ProgressionLevelsData.LevelData levelData) {
            this.levelData = levelData;
            icon.sprite = levelData.sprite;
            label.text = levelData.name;
        }

        [Button]
        public void SetCompleted() {
            canvasGroup.alpha = 1f;
            currentBackground.enabled = false;
            rectTransform.SetHeight(standardHeight);
            progressBarContent.SetActive(false);
        }

        [Button]
        public void SetCurrent() {
            canvasGroup.alpha = 1f;
            currentBackground.enabled = true;
            rectTransform.SetHeight(currentHeight);
            progressBarContent.SetActive(true);
            
        }

        public void UpdateAmount(float currentAmount) {
            progressBarFillImage.fillAmount = currentAmount / Value;
            progressBarAmountLabel.text = $"{currentAmount} / {Value}";
        }

        [Button]
        public void SetFuture() {
            canvasGroup.alpha = futureAlpha;
            currentBackground.enabled = false;
            rectTransform.SetHeight(standardHeight);
            progressBarContent.SetActive(false);
        }
    }
}