using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation.Factories {
    public class FactoryIconView : MonoBehaviour {
        [SerializeField] Image factoryIcon;
        [SerializeField] RectTransform amountPanel;
        [SerializeField] TMP_Text amountLabel;
        [SerializeField] Vector2 rebuildSizeDelta;

        public void SetIcon(Sprite icon) {
            factoryIcon.sprite = icon;
        }

        public void SetAmount(int factoryLevel) {
            amountPanel.sizeDelta = rebuildSizeDelta;
            amountLabel.text = $"{factoryLevel}";
            amountLabel.ForceMeshUpdate();
            Vector2 preferred = amountLabel.GetPreferredValues();
            amountPanel.sizeDelta = preferred;
        }
    }
}