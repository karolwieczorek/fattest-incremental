using UnityEngine;
using UnityEngine.EventSystems;

namespace FattestInc.UI.API {
    public abstract class HUiEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
        IPointerExitHandler, IPointerClickHandler {
        
        public virtual void OnPointerDown(PointerEventData eventData) { }

        public virtual void OnPointerUp(PointerEventData eventData) { }

        public virtual void OnPointerEnter(PointerEventData eventData) { }

        public virtual void OnPointerExit(PointerEventData eventData) { }

        public virtual void OnPointerClick(PointerEventData eventData) { }
    }
}