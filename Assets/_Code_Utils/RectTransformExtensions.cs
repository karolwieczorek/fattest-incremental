using UnityEngine;

namespace Hypnagogia.Utils {
    public static class RectTransformExtensions {
        public static void SetHeight(this RectTransform rectTransform, float height) {
            Vector2 size = rectTransform.sizeDelta;
            size.y = height;
            rectTransform.sizeDelta = size;
        }
    }
}