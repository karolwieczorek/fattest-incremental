using UnityEngine;

namespace Hypnagogia.Utils {
    public static class UnityEngineObjectExtensions {
        public static void Destroy(this Object unityObject) {
            if (Application.isEditor && Application.isPlaying == false) {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(unityObject);
#endif
            }
            else {
                Object.Destroy(unityObject);
            }
        }
    }
}