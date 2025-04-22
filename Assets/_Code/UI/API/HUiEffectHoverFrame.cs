using UnityEngine;
using UnityEngine.EventSystems;

namespace FattestInc.UI.API {
    public class HUiEffectHoverFrame : HUiEffect {
        [SerializeField] GameObject hoverFrame;

        public override void OnPointerEnter(PointerEventData eventData) {
            hoverFrame.SetActive(true);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            hoverFrame.SetActive(false);
        }
    }
}