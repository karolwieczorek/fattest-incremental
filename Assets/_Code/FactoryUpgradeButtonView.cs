using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc {
    public class FactoryUpgradeButtonView : MonoBehaviour {
        [SerializeField] Image buttonBackgroundImage;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] TMP_Text costText;

        [SerializeField] Data available;
        [SerializeField] Data unavailable;
        [SerializeField] Data max;

        [Button]
        [ContextMenu(nameof(ApplyAvailable))]
        public void ApplyAvailable() {
            Apply(available);
        }

        [Button]
        [ContextMenu(nameof(ApplyUnavailable))]
        public void ApplyUnavailable() {
            Apply(unavailable);
        }

        [Button]
        [ContextMenu(nameof(ApplyMax))]
        public void ApplyMax() {
            Apply(max);
        }

        void Apply(Data data) {
            buttonBackgroundImage.color = data.backgroundColor;
            buttonText.color = data.buttonTextColor;
            costText.color = data.costTextColor;
        }

        [System.Serializable]
        public class Data {
            public Color backgroundColor;
            public Color buttonTextColor;
            public Color costTextColor;
        }
    }
}