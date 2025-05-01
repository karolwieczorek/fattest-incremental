using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FattestInc.UI.Implementation {
    public class DropdownHoverHandler : MonoBehaviour, IPointerEnterHandler {
        TMP_Dropdown dropdown;
        GameObject dropdownList;

        void Awake() {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (!dropdown.IsExpanded) {
                dropdown.Show();
            }
        }
    }
}