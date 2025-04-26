using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation {
    public class FactoryUpgradeButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] Image buttonBackgroundImage;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] TMP_Text costText;
        [SerializeField] Button button;

        [SerializeField] Data available;
        [SerializeField] Data unavailable;
        [SerializeField] Data max;

        bool isHovered;
        public bool IsHovered => button.interactable && isHovered;
        public event Action PointerEnter;
        public event Action PointerExit;

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
            button.interactable = data.buttonInteractable;
        }

        [System.Serializable]
        public class Data {
            public Color backgroundColor;
            public Color buttonTextColor;
            public Color costTextColor;
            public bool buttonInteractable;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            isHovered = true;
            if (button.interactable)
                PointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) {
            isHovered = false;
            if (button.interactable)
                PointerExit?.Invoke();
        }
    }
}