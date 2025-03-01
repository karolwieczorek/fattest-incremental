using UnityEngine;

namespace Hypnagogia.Utils {
    public static class GameObjectExtensions {
        public static void DestroyChildren(this GameObject gameObject) {
            var parent = gameObject.transform;
            parent.DestroyChildren();
        }
    }
}