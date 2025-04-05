using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FattestInc {
    public class ProgressionLevelView : MonoBehaviour, IInitializable {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float futureAlpha = 0.5f;

        [HInject] EconomyDataStore economyDataStore;

        ProgressionLevelsData.LevelData levelData;

        public ulong Value => levelData?.value ?? 0;

        public void Init(ProgressionLevelsData.LevelData levelData) {
            this.levelData = levelData;
            icon.sprite = levelData.sprite;
            label.text = levelData.name;
        }

        public void Initialize() {
            economyDataStore.CurrentTotalAmount.ChangedValue += OnCurrentAmountChanged;
        }

        void OnCurrentAmountChanged(ulong value) {
            
        }

        public void SetCompleted() {
            canvasGroup.alpha = 1f;
        }

        public void SetCurrent() {
            canvasGroup.alpha = 1f;
        }

        public void SetFuture() {
            canvasGroup.alpha = futureAlpha;
        }
    }
}